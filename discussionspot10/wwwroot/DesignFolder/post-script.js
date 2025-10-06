// Handle post voting
function handlePostVote(button, type) {
  const votingSidebar = button.closest(".post-voting-sidebar")
  const upvoteBtn = votingSidebar.querySelector(".upvote")
  const downvoteBtn = votingSidebar.querySelector(".downvote")
  const voteCountSpan = votingSidebar.querySelector(".vote-count-large")

  let currentCount = Number.parseFloat(voteCountSpan.textContent.replace("k", "")) * 1000
  const isActive = button.classList.contains("active")

  // Remove active class from both buttons
  upvoteBtn.classList.remove("active")
  downvoteBtn.classList.remove("active")
  voteCountSpan.classList.remove("positive", "negative")

  if (!isActive) {
    // Activate clicked button
    button.classList.add("active")

    if (type === "upvote") {
      currentCount += 1
      if (currentCount > 0) voteCountSpan.classList.add("positive")
    } else {
      currentCount -= 1
      if (currentCount < 0) voteCountSpan.classList.add("negative")
    }
  }

  // Format the count
  if (currentCount >= 1000) {
    voteCountSpan.textContent = (currentCount / 1000).toFixed(1) + "k"
  } else {
    voteCountSpan.textContent = currentCount.toString()
  }
}

// Handle comment voting
function handleCommentVote(button, type) {
  const commentVoting = button.closest(".comment-voting")
  const upvoteBtn = commentVoting.querySelector(".upvote")
  const downvoteBtn = commentVoting.querySelector(".downvote")
  const voteCountSpan = commentVoting.querySelector(".comment-vote-count")

  let currentCount = Number.parseInt(voteCountSpan.textContent) || 0
  const isActive = button.classList.contains("active")

  // Remove active class from both buttons
  upvoteBtn.classList.remove("active")
  downvoteBtn.classList.remove("active")
  voteCountSpan.classList.remove("positive", "negative")

  if (!isActive) {
    // Activate clicked button
    button.classList.add("active")

    if (type === "upvote") {
      currentCount += 1
      if (currentCount > 0) voteCountSpan.classList.add("positive")
    } else {
      currentCount -= 1
      if (currentCount < 0) voteCountSpan.classList.add("negative")
    }
  }

  voteCountSpan.textContent = currentCount
}

// Handle poll voting
function voteOnPoll(optionElement) {
  // Remove active class from all options
  document.querySelectorAll(".poll-option-detailed").forEach((option) => {
    option.classList.remove("active")
  })

  // Add active class to clicked option
  optionElement.classList.add("active")

  console.log("[v0] Poll vote registered")
}

// Handle image gallery
function changeGalleryImage(thumbnail) {
  const mainImage = document.getElementById("gallery-main-image")
  const allThumbnails = document.querySelectorAll(".gallery-thumbnail")

  // Remove active class from all thumbnails
  allThumbnails.forEach((thumb) => thumb.classList.remove("active"))

  // Add active class to clicked thumbnail
  thumbnail.classList.add("active")

  // Change main image
  const newImageSrc = thumbnail.src.replace("w=200", "w=1200")
  mainImage.src = newImageSrc
}

// Utilities for new bottom vote bars
function v0_parseCount(text) {
  if (!text) return 0
  const s = String(text).trim().toLowerCase()
  if (s.endsWith("k")) return Math.round(Number.parseFloat(s) * 1000)
  if (s.endsWith("m")) return Math.round(Number.parseFloat(s) * 1_000_000)
  const n = Number.parseInt(s.replace(/[^0-9-]/g, ""), 10)
  return Number.isNaN(n) ? 0 : n
}

function v0_formatCount(n) {
  const abs = Math.abs(n)
  if (abs >= 1_000_000) return (n / 1_000_000).toFixed(abs >= 10_000_000 ? 0 : 1) + "m"
  if (abs >= 1_000) return (n / 1_000).toFixed(abs >= 10_000 ? 0 : 1) + "k"
  return String(n)
}

function v0_bindVoteBar(root) {
  if (!root) return
  const countEl = root.querySelector(".vote-count")
  let count = countEl?.dataset.count ? Number.parseInt(countEl.dataset.count, 10) : v0_parseCount(countEl?.textContent)
  countEl.textContent = v0_formatCount(count)

  const up = root.querySelector(".vote-btn.up")
  const down = root.querySelector(".vote-btn.down")
  if (!up || !down || !countEl) return

  const setActive = (which) => {
    up.classList.toggle("active", which === "up")
    down.classList.toggle("active", which === "down")
  }

  up.addEventListener("click", () => {
    const wasActive = up.classList.contains("active")
    setActive(wasActive ? null : "up")
    count += wasActive ? -1 : 1
    countEl.textContent = v0_formatCount(count)
  })

  down.addEventListener("click", () => {
    const wasActive = down.classList.contains("active")
    setActive(wasActive ? null : "down")
    count += wasActive ? 1 : -1
    countEl.textContent = v0_formatCount(count)
  })
}

// Initialize page
document.addEventListener("DOMContentLoaded", () => {
  console.log("[v0] Post page loaded")

  // Handle comment reply buttons
  document.querySelectorAll(".comment-action-btn").forEach((btn) => {
    if (btn.textContent.includes("Reply")) {
      btn.addEventListener("click", () => {
        console.log("[v0] Reply button clicked")
        // Add reply functionality here
      })
    }
  })

  // Handle load more comments
  const loadMoreBtn = document.querySelector(".load-more-comments-btn")
  if (loadMoreBtn) {
    loadMoreBtn.addEventListener("click", () => {
      console.log("[v0] Loading more comments...")
      // Add load more functionality here
    })
  }

  // Remove legacy vertical stacks to prevent duplicates
  document.querySelectorAll(".comment-voting").forEach((el) => el.remove())

  // Wrap actions + bottom vote bar into a single footer row if not already
  document.querySelectorAll(".comment-content").forEach((content) => {
    const actions = content.querySelector(".comment-actions")
    const bar = content.querySelector(".comment-vote-bar")
    // Create footer wrapper if missing
    if (actions && bar && !bar.parentElement.classList.contains("comment-footer")) {
      const footer = document.createElement("div")
      footer.className = "comment-footer"
      actions.after(footer)
      footer.appendChild(actions)
      footer.appendChild(bar)
    }
    // Ensure a reply form container exists
    if (!content.querySelector(".reply-form-container")) {
      const c = document.createElement("div")
      c.className = "reply-form-container"
      c.hidden = true
      content.appendChild(c)
    }
  })

  // Bind vote bars (post + comments)
  v0_bindVoteBar(document.querySelector(".post-vote-bar"))
  document.querySelectorAll(".comment-vote-bar").forEach(v0_bindVoteBar)

  // Reply form toggle and submit
  const commentsRoot = document.querySelector(".comments-list")
  if (commentsRoot) {
    commentsRoot.addEventListener("click", (e) => {
      const btn = e.target.closest(".comment-action-btn")
      if (!btn) return
      const isReply = btn.dataset.action === "reply" || /reply/i.test(btn.textContent)
      if (!isReply) return

      const content = btn.closest(".comment-content")
      const container = content.querySelector(".reply-form-container")
      if (!container) return

      if (container.childElementCount === 0) {
        container.appendChild(createReplyForm())
      }
      container.hidden = !container.hidden
    })
  }
})

// Create inline reply form element
function createReplyForm() {
  const wrapper = document.createElement("div")
  wrapper.className = "reply-form"
  wrapper.innerHTML = `
    <textarea placeholder="Write your reply..."></textarea>
    <div class="reply-actions">
      <button type="button" class="btn btn-sm btn-outline-secondary reply-cancel">Cancel</button>
      <button type="button" class="btn btn-sm btn-primary reply-submit">Reply</button>
    </div>
  `
  wrapper.addEventListener("click", (e) => {
    if (e.target.classList.contains("reply-cancel")) {
      wrapper.parentElement.hidden = true
    }
    if (e.target.classList.contains("reply-submit")) {
      const text = wrapper.querySelector("textarea").value.trim()
      if (!text) return
      // You can replace this with real submission logic
      console.log("[v0] Reply submitted:", text)
      wrapper.querySelector("textarea").value = ""
      wrapper.parentElement.hidden = true
    }
  })
  return wrapper
}
