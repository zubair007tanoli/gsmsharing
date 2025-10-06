;(() => {
  const ready = (fn) => (document.readyState !== "loading" ? fn() : document.addEventListener("DOMContentLoaded", fn))

  ready(() => {
    // 1) Remove stray debug labels rendering as text
    const strayTexts = new Set([
      "Main Content",
      "Main Post Column",
      "Post Detail Card",
      "Post Header",
      "Post Content",
      "Right Sidebar",
    ])

    // Remove standalone text nodes
    ;[...document.body.childNodes].forEach((n) => {
      if (n.nodeType === Node.TEXT_NODE && strayTexts.has(n.textContent.trim())) {
        n.remove()
      }
    })
    // Remove elements that only contain the stray labels
    document.querySelectorAll("body *").forEach((el) => {
      if (el.children.length === 0 && strayTexts.has(el.textContent.trim())) el.remove()
    })

    // Helpers
    const parseCount = (t) => {
      if (!t) return 0
      const s = ("" + t).trim().toLowerCase()
      if (s.endsWith("k")) return Math.round(Number.parseFloat(s) * 1000)
      if (s.endsWith("m")) return Math.round(Number.parseFloat(s) * 1000_000)
      const n = Number.parseInt(s.replace(/[^0-9-]/g, ""), 10)
      return isNaN(n) ? 0 : n
    }
    const formatCount = (n) => {
      const abs = Math.abs(n)
      if (abs >= 1_000_000) return (n / 1_000_000).toFixed(abs >= 10_000_000 ? 0 : 1) + "m"
      if (abs >= 1000) return (n / 1000).toFixed(abs >= 10_000 ? 0 : 1) + "k"
      return String(n)
    }
    const svg = {
      up: '<svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 5l7 8h-4v6H9v-6H5z"/></svg>',
      down: '<svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 19l-7-8h4V5h6v6h4z"/></svg>',
    }

    const makeBar = (count, onChange) => {
      const wrap = document.createElement("div")
      wrap.className = "vote-bar"
      const upBtn = document.createElement("button")
      upBtn.className = "vote-btn up"
      upBtn.innerHTML = svg.up
      const downBtn = document.createElement("button")
      downBtn.className = "vote-btn down"
      downBtn.innerHTML = svg.down
      const cnt = document.createElement("span")
      cnt.className = "vote-count"
      cnt.textContent = formatCount(count)

      let current = count
      const set = (n) => {
        current = n
        cnt.textContent = formatCount(current)
        onChange && onChange(current)
      }
      upBtn.addEventListener("click", () => set(current + 1))
      downBtn.addEventListener("click", () => set(current - 1))

      wrap.appendChild(upBtn)
      wrap.appendChild(cnt)
      wrap.appendChild(downBtn)
      return { wrap, set }
    }

    // 2) POST: find post container and any legacy left vote area
    const postRoot =
      document.querySelector(".post-detail, .post-card, .post, article.post") ||
      document.querySelector("article") ||
      document.body

    const legacyVote =
      postRoot.querySelector(".vote-sidebar, .vote-column, .left-vote, .post-votes, .score-area, .voting") || null

    // try read current count
    let startCount = 0
    if (legacyVote) {
      const possibleCount = legacyVote.querySelector(".score, .count, .vote-count") || legacyVote
      startCount = parseCount(possibleCount.textContent)
    }

    // append new bottom bar near body/content
    const contentArea = postRoot.querySelector(".post-content, .post-body, .content, .card-body, .entry") || postRoot

    const { wrap: postBar } = makeBar(startCount, (n) => {
      // no-op – tie into backend as needed
    })
    postBar.classList.add("vote-bar")
    // ensure it goes before comments if possible
    const commentsAnchor = document.querySelector("#comments, .comments, .comment-list, .comments-area")
    if (commentsAnchor && commentsAnchor.parentElement === postRoot) {
      postRoot.insertBefore(postBar, commentsAnchor)
    } else {
      contentArea.appendChild(postBar)
    }

    if (legacyVote) legacyVote.style.display = "none"

    // 3) COMMENTS: move each comment’s votes to bottom
    const commentNodes = document.querySelectorAll(".comment, .comment-item, .comment-card, li.comment")
    commentNodes.forEach((c) => {
      const cLegacy =
        c.querySelector(".vote-sidebar, .vote-column, .left-vote, .comment-votes, .score-area, .voting") || null

      let cCount = 0
      if (cLegacy) {
        const pc = cLegacy.querySelector(".score, .count, .vote-count") || cLegacy
        cCount = parseCount(pc.textContent)
      }
      const { wrap } = makeBar(cCount)
      wrap.classList.remove("vote-bar")
      wrap.classList.add("comment-vote-bar")
      c.appendChild(wrap)
      if (cLegacy) cLegacy.style.display = "none"
    })
  })
})()
