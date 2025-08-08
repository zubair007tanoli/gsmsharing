/*!
 * PollHub Client
 * Connects to /pollHub, joins per-post groups, casts votes, and updates the UI in real-time.
 *
 * Requirements:
 * - Include SignalR client BEFORE this file:
 *   <script src="/lib/microsoft/signalr/dist/browser/signalr.js"></script>
 * - Include (optional) Bootstrap JS for toasts, or the fallback will log to console/alert.
 *
 * Usage:
 * 1) Include this file on pages that render a poll.
 * 2) Ensure your poll markup has:
 *    - A container per option:   <div class="poll-option" data-option-id="{id}"> ... </div>
 *    - The count element:        <span class="poll-option-count"></span>
 *    - The percent element:      <span class="poll-option-percent"></span>        (optional)
 *    - The progress bar:         <div class="poll-option-bar"></div>             (optional; width set as %)
 *    - A vote button:            <button class="poll-option-vote-btn" data-post-id="{postId}" data-option-id="{optionId}">Vote</button>
 *
 * 3) Initialize automatically if the page provides the post id via:
 *    - <input type="hidden" id="pagePostId" value="{postId}" />
 *    or any element with data-post-id="{postId}"
 *
 * Or initialize manually:
 *    window.pollManager = new PollManager();
 *    pollManager.init(postId);
 */

(function () {
  class PollManager {
    constructor() {
      this.connection = null;
      this.postId = null;

      this.handleVoteClick = this.handleVoteClick.bind(this);
      this.onReceivePollUpdate = this.onReceivePollUpdate.bind(this);
      this.onReceivePollVoteSuccess = this.onReceivePollVoteSuccess.bind(this);
      this.onReceivePollVoteError = this.onReceivePollVoteError.bind(this);

      // Used to avoid rebinding listeners repeatedly
      this.boundHandlerAttribute = "data-poll-handler";
    }

    async init(postId) {
      // Validate SignalR
      if (typeof signalR === "undefined" || !signalR.HubConnectionBuilder) {
        this.notify("SignalR client library is missing. Poll updates will not work.", "error");
        console.error("PollManager: signalR client not found. Ensure the script is included before this file.");
        return;
      }

      // Save postId
      this.postId = Number(postId);
      if (!this.postId || Number.isNaN(this.postId)) {
        this.notify("PollManager init: invalid post id.", "error");
        console.error("PollManager: Invalid postId passed to init:", postId);
        return;
      }

      // Build connection
      try {
        this.connection = new signalR.HubConnectionBuilder()
          .withUrl("/pollHub")
          .withAutomaticReconnect()
          .build();

        // Register handlers
        this.registerHandlers();

        // Start connection
        await this.connection.start();
        console.info("PollManager: Connected to /pollHub. Connection state:", this.connection.state);

        // Join post group
        await this.joinPostGroup(this.postId);

        // Bind click handlers
        this.bindVoteButtons(this.postId);

        // Rejoin on reconnect
        this.connection.onreconnected(async () => {
          console.info("PollManager: Reconnected. Rejoining group for post:", this.postId);
          await this.joinPostGroup(this.postId);
        });
      } catch (err) {
        console.error("PollManager: Failed to start SignalR connection.", err);
        this.notify("Unable to connect to poll updates. Try refreshing the page.", "error");
      }
    }

    registerHandlers() {
      if (!this.connection) return;

      this.connection.on("ReceivePollUpdate", this.onReceivePollUpdate);
      this.connection.on("ReceivePollVoteSuccess", this.onReceivePollVoteSuccess);
      this.connection.on("ReceivePollVoteError", this.onReceivePollVoteError);

      this.connection.onclose((err) => {
        console.warn("PollManager: Connection closed.", err);
      });
      this.connection.onreconnecting((err) => {
        console.warn("PollManager: Reconnecting...", err);
      });
    }

    async joinPostGroup(postId) {
      if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
        console.warn("PollManager: Cannot join group; connection not ready.");
        return;
      }
      try {
        await this.connection.invoke("JoinPostGroup", postId);
        console.info(`PollManager: Joined group post-${postId}`);
      } catch (err) {
        console.error("PollManager: Failed to join post group:", err);
      }
    }

    bindVoteButtons(postId) {
      const selector = `.poll-option-vote-btn[data-post-id="${postId}"]`;
      document.querySelectorAll(selector).forEach((btn) => {
        // Avoid double-binding (coexist with other scripts)
        if (btn.getAttribute(this.boundHandlerAttribute) === "true") return;

        btn.addEventListener("click", this.handleVoteClick);
        btn.setAttribute(this.boundHandlerAttribute, "true");
      });
    }

    async handleVoteClick(event) {
      event.preventDefault();
      const btn = event.currentTarget;
      const postId = Number(btn.getAttribute("data-post-id"));
      const optionId = Number(btn.getAttribute("data-option-id"));

      if (!postId || Number.isNaN(postId) || !optionId || Number.isNaN(optionId)) {
        this.notify("Invalid poll vote parameters.", "error");
        return;
      }

      await this.castPollVote(postId, optionId);

      // UX: disable all vote buttons for this post after a successful cast attempt
      // (Server will enforce single-choice; this is optimistic. Buttons re-enable on error.)
      try {
        document
          .querySelectorAll(`.poll-option-vote-btn[data-post-id="${postId}"]`)
          .forEach((b) => {
            b.disabled = true;
            b.classList.add("disabled");
          });
      } catch (e) {
        console.warn("PollManager: could not disable vote buttons after click.", e);
      }
    }

    async castPollVote(postId, optionId) {
      if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
        this.notify("Not connected. Please check your internet connection.", "error");
        return;
      }
      try {
        await this.connection.invoke("CastPollVote", postId, optionId);
        // Server will respond with ReceivePollVoteSuccess and broadcast ReceivePollUpdate.
      } catch (err) {
        console.error("PollManager: Error invoking CastPollVote:", err);
        this.notify("Failed to cast vote. Please try again.", "error");
        // Re-enable buttons on error to allow retry
        try {
          document
            .querySelectorAll(`.poll-option-vote-btn[data-post-id="${postId}"]`)
            .forEach((b) => {
              b.disabled = false;
              b.classList.remove("disabled");
            });
        } catch {}
      }
    }

    // Server event handlers
    onReceivePollUpdate(updatedVoteCounts) {
      try {
        this.updatePollUI(updatedVoteCounts);
      } catch (err) {
        console.error("PollManager: Error updating poll UI:", err);
      }
    }

    onReceivePollVoteSuccess(message) {
      this.notify(message || "Your vote was recorded successfully!", "success");
    }

    onReceivePollVoteError(message) {
      this.notify(message || "Failed to cast your vote.", "error");
      // Re-enable buttons to allow retry
      if (this.postId) {
        try {
          document
            .querySelectorAll(`.poll-option-vote-btn[data-post-id="${this.postId}"]`)
            .forEach((b) => {
              b.disabled = false;
              b.classList.remove("disabled");
            });
        } catch {}
      }
    }

    // UI update logic
    updatePollUI(updated) {
      // Normalize input to array of { id, votes, percentage? }
      let items = [];
      if (Array.isArray(updated)) {
        items = updated
          .map((it) => {
            const id =
              it.pollOptionId ?? it.PollOptionId ??
              it.optionId ?? it.OptionId ??
              it.id ?? it.Id;
            const votes =
              it.voteCount ?? it.VoteCount ??
              it.votes ?? it.Votes ??
              it.count ?? it.Count ?? 0;
            const percentage = it.percentage ?? it.Percentage ?? null;
            return id ? { id: Number(id), votes: Number(votes) || 0, percentage } : null;
          })
          .filter(Boolean);
      } else if (updated && typeof updated === "object") {
        items = Object.keys(updated).map((k) => ({
          id: Number(k),
          votes: Number(updated[k]) || 0,
          percentage: null,
        }));
      }

      const totalVotes = items.reduce((sum, i) => sum + (Number(i.votes) || 0), 0);

      items.forEach(({ id, votes, percentage }) => {
        const row =
          document.querySelector(`.poll-option[data-option-id="${id}"]`) ||
          document.getElementById(`pollOption-${id}`);
        if (!row) return;

        const countEl =
          row.querySelector(".poll-option-count") ||
          document.getElementById(`pollOptionCount-${id}`);
        if (countEl) countEl.textContent = String(votes);

        const pct = percentage != null
          ? Math.round(Number(percentage))
          : (totalVotes > 0 ? Math.round((votes / totalVotes) * 100) : 0);

        const bar = row.querySelector(".poll-option-bar") || row.querySelector(".progress-bar");
        if (bar) {
          bar.style.width = `${pct}%`;
          bar.setAttribute("aria-valuenow", String(pct));
        }

        const percentEl = row.querySelector(".poll-option-percent");
        if (percentEl) percentEl.textContent = `${pct}%`;
      });

      // Optional: update a total votes label if present
      const totalEl = document.querySelector(".poll-total-votes");
      if (totalEl) totalEl.textContent = String(totalVotes);
    }

    // Notifications (Bootstrap toast if available; fallback to alert/console)
    notify(message, type = "info") {
      try {
        const toastContainerId = "poll-toast-container";
        let container = document.getElementById(toastContainerId);
        if (!container) {
          container = document.createElement("div");
          container.id = toastContainerId;
          container.style.position = "fixed";
          container.style.top = "1rem";
          container.style.right = "1rem";
          container.style.zIndex = "1060";
          document.body.appendChild(container);
        }

        const toastEl = document.createElement("div");
        const clazz =
          type === "success" ? "bg-success" :
          type === "error" ? "bg-danger" : "bg-info";
        toastEl.className = `toast text-white ${clazz} border-0`;
        toastEl.setAttribute("role", "alert");
        toastEl.setAttribute("aria-live", "assertive");
        toastEl.setAttribute("aria-atomic", "true");
        toastEl.innerHTML = `
          <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
          </div>
        `;
        container.appendChild(toastEl);

        if (window.bootstrap && window.bootstrap.Toast) {
          const t = new window.bootstrap.Toast(toastEl, { delay: 3000 });
          t.show();
          toastEl.addEventListener("hidden.bs.toast", () => toastEl.remove());
        } else {
          // Fallback if Bootstrap isn't present
          console.log(`[${type.toUpperCase()}] ${message}`);
          // Simple auto-remove
          setTimeout(() => toastEl.remove(), 3000);
        }
      } catch (e) {
        console.log(`[${type.toUpperCase()}] ${message}`);
        try {
          if (type === "error") alert(message);
        } catch {}
      }
    }
  }

  // Expose globally
  window.PollManager = PollManager;

  // Auto-init if we can infer post id on the page
  document.addEventListener("DOMContentLoaded", async () => {
    try {
      const postIdEl = document.getElementById("pagePostId") || document.querySelector("[data-post-id]");
      const inferredPostId = postIdEl
        ? Number(postIdEl.value || postIdEl.getAttribute("data-post-id"))
        : null;

      if (inferredPostId && !Number.isNaN(inferredPostId)) {
        const mgr = new PollManager();
        window.pollManager = mgr;
        await mgr.init(inferredPostId);
      }
    } catch (e) {
      console.warn("PollManager auto-init skipped.", e);
    }
  });
})();