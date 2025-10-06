// Categories data
const categories = [
  { name: "Technology", icon: "fa-microchip", color: "#0079d3" },
  { name: "Gaming", icon: "fa-gamepad", color: "#7c3aed" },
  { name: "Science", icon: "fa-flask", color: "#059669" },
  { name: "Movies", icon: "fa-film", color: "#dc2626" },
  { name: "Sports", icon: "fa-basketball", color: "#ea580c" },
  { name: "Food", icon: "fa-utensils", color: "#d97706" },
  { name: "Travel", icon: "fa-plane", color: "#0891b2" },
  { name: "Finance", icon: "fa-chart-line", color: "#16a34a" },
  { name: "Health", icon: "fa-heart-pulse", color: "#e11d48" },
  { name: "Art", icon: "fa-palette", color: "#9333ea" },
  { name: "Music", icon: "fa-music", color: "#ec4899" },
  { name: "Books", icon: "fa-book", color: "#8b5cf6" },
  { name: "Fashion", icon: "fa-shirt", color: "#f43f5e" },
  { name: "DIY", icon: "fa-hammer", color: "#f59e0b" },
  { name: "Pets", icon: "fa-paw", color: "#84cc16" },
  { name: "Photography", icon: "fa-camera", color: "#06b6d4" },
  { name: "Education", icon: "fa-graduation-cap", color: "#3b82f6" },
  { name: "Business", icon: "fa-briefcase", color: "#64748b" },
]

// Communities data
const communities = [
  { name: "r/webdev", icon: "w", members: "1.5M", category: "Technology" },
  { name: "r/programming", icon: "p", members: "5.2M", category: "Technology" },
  { name: "r/technology", icon: "t", members: "14.8M", category: "Technology" },
  { name: "r/gaming", icon: "g", members: "36.7M", category: "Gaming" },
  { name: "r/pcgaming", icon: "p", members: "2.8M", category: "Gaming" },
  { name: "r/science", icon: "s", members: "29.3M", category: "Science" },
  { name: "r/space", icon: "s", members: "23.1M", category: "Science" },
  { name: "r/movies", icon: "m", members: "28.9M", category: "Movies" },
  { name: "r/television", icon: "t", members: "17.6M", category: "Movies" },
  { name: "r/sports", icon: "s", members: "9.2M", category: "Sports" },
  { name: "r/nba", icon: "n", members: "8.4M", category: "Sports" },
  { name: "r/food", icon: "f", members: "24.5M", category: "Food" },
  { name: "r/cooking", icon: "c", members: "5.8M", category: "Food" },
  { name: "r/travel", icon: "t", members: "19.2M", category: "Travel" },
  { name: "r/finance", icon: "f", members: "3.1M", category: "Finance" },
  { name: "r/investing", icon: "i", members: "2.4M", category: "Finance" },
  { name: "r/fitness", icon: "f", members: "10.7M", category: "Health" },
  { name: "r/art", icon: "a", members: "22.4M", category: "Art" },
  { name: "r/music", icon: "m", members: "32.1M", category: "Music" },
  { name: "r/books", icon: "b", members: "21.8M", category: "Books" },
  { name: "r/fashion", icon: "f", members: "4.2M", category: "Fashion" },
  { name: "r/DIY", icon: "d", members: "21.5M", category: "DIY" },
  { name: "r/pets", icon: "p", members: "4.9M", category: "Pets" },
  { name: "r/photography", icon: "p", members: "4.1M", category: "Photography" },
  { name: "r/learnprogramming", icon: "l", members: "4.8M", category: "Education" },
  { name: "r/entrepreneur", icon: "e", members: "2.9M", category: "Business" },
  { name: "r/artificialintelligence", icon: "a", members: "1.8M", category: "Technology" },
  { name: "r/cryptocurrency", icon: "c", members: "6.5M", category: "Finance" },
  { name: "r/virtualreality", icon: "v", members: "1.2M", category: "Technology" },
  { name: "r/datascience", icon: "d", members: "1.1M", category: "Technology" },
]

// Posts data with varied content
const postsData = [
  {
    community: "r/technology",
    author: "u/techreview",
    time: "5 hours ago",
    type: "Image",
    title: "iPhone 15 Pro Max Battery Life Exceeds Expectations in Latest Tests",
    body: "",
    image: "https://images.unsplash.com/photo-1678911820864-e2c567c655d7?q=80&w=1000",
    upvotes: 1768,
    comments: 142,
    shares: 234,
    tags: ["Apple", "iPhone", "Battery"],
  },
  {
    community: "r/programming",
    author: "u/devmaster",
    time: "8 hours ago",
    type: "Poll",
    title: "What's your favorite programming language in 2025?",
    body: "",
    image: null,
    upvotes: 908,
    comments: 328,
    shares: 89,
    tags: ["Programming", "Poll"],
    poll: {
      options: [
        { text: "Python", votes: 1234, percentage: 35 },
        { text: "JavaScript/TypeScript", votes: 987, percentage: 28 },
        { text: "Rust", votes: 654, percentage: 19 },
        { text: "Go", votes: 432, percentage: 12 },
        { text: "Other", votes: 210, percentage: 6 },
      ],
      totalVotes: 3517,
      endsIn: "5 days left",
    },
  },
  {
    community: "r/science",
    author: "u/sciencenews",
    time: "12 hours ago",
    type: "Link",
    title: "New Study Reveals Breakthrough in Quantum Computing Error Correction",
    body: "",
    image: "https://images.unsplash.com/photo-1635070041078-e363dbe005cb?q=80&w=500",
    link: "nature.com",
    upvotes: 3124,
    comments: 187,
    shares: 456,
    tags: ["Quantum", "Research"],
  },
  {
    community: "r/announcements",
    author: "u/admin",
    time: "2 hours ago",
    type: "Announcement",
    title: "Introducing Community Awards: Recognize great contributions!",
    body: "We're excited to announce a new way to celebrate outstanding community members. Starting today, you can give special awards to posts and comments that make a difference.",
    image: null,
    upvotes: 8934,
    comments: 456,
    shares: 1567,
    tags: ["Announcement", "Features"],
    isPinned: true,
  },
  {
    community: "r/gaming",
    author: "u/gamerhero",
    time: "3 hours ago",
    type: "Video",
    title: "This new GTA 6 trailer analysis reveals hidden details you definitely missed",
    body: "",
    image: "https://images.unsplash.com/photo-1493711662062-fa541adb3fc8?q=80&w=1000",
    upvotes: 5576,
    comments: 842,
    shares: 1203,
    tags: ["GTA6", "Gaming", "Analysis"],
  },
  {
    community: "r/movies",
    author: "u/filmcritic",
    time: "6 hours ago",
    type: "Discussion",
    title: "Unpopular Opinion: Dune Part Two is better than the first movie",
    body: "Just saw Dune Part Two and I have to say it surpasses the first film in almost every way. The character development, action sequences, and visual effects are all significantly improved. What do you think?",
    image: null,
    upvotes: 1085,
    comments: 476,
    shares: 167,
    tags: ["Dune", "Movies", "Opinion"],
  },
  {
    community: "Sponsored",
    author: "u/TechBrand",
    time: "Promoted",
    type: "Ad",
    title: "Upgrade Your Workspace with Premium Monitors - 30% Off Today",
    body: "Experience crystal-clear 4K resolution and ergonomic design. Limited time offer for DiscussHub members.",
    image: "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?q=80&w=1000",
    upvotes: 0,
    comments: 0,
    shares: 0,
    tags: [],
    isAd: true,
  },
  {
    community: "r/artificialintelligence",
    author: "u/airesearcher",
    time: "2 hours ago",
    type: "Discussion",
    title: "GPT-5 rumors: What we know so far about OpenAI's next model",
    body: "There's been a lot of speculation about GPT-5. Let's compile what we actually know from reliable sources versus what's just speculation.",
    image: null,
    upvotes: 2341,
    comments: 567,
    shares: 789,
    tags: ["AI", "GPT", "OpenAI"],
  },
  {
    community: "r/space",
    author: "u/spacefan",
    time: "4 hours ago",
    type: "Image",
    title: "James Webb Telescope captures stunning image of distant galaxy cluster",
    body: "",
    image: "https://images.unsplash.com/photo-1614732414444-096e5f1122d5?q=80&w=1000",
    upvotes: 4567,
    comments: 234,
    shares: 892,
    tags: ["Space", "JWST", "Astronomy"],
  },
  {
    community: "r/webdev",
    author: "u/fullstackdev",
    time: "7 hours ago",
    type: "Poll",
    title: "Which framework will dominate in 2026?",
    body: "",
    image: null,
    upvotes: 1456,
    comments: 623,
    shares: 234,
    tags: ["WebDev", "Poll", "Frontend"],
    poll: {
      options: [
        { text: "React", votes: 2341, percentage: 42 },
        { text: "Vue", votes: 1234, percentage: 22 },
        { text: "Svelte", votes: 987, percentage: 18 },
        { text: "Angular", votes: 654, percentage: 12 },
        { text: "Solid", votes: 334, percentage: 6 },
      ],
      totalVotes: 5550,
      endsIn: "3 days left",
    },
  },
  {
    community: "r/fitness",
    author: "u/fitnessguru",
    time: "9 hours ago",
    type: "Image",
    title: "6-month transformation: From couch potato to marathon runner",
    body: "Never thought I could do it, but here I am! Consistency is key.",
    image: "https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?q=80&w=1000",
    upvotes: 8934,
    comments: 456,
    shares: 1567,
    tags: ["Fitness", "Transformation", "Motivation"],
  },
  {
    community: "Sponsored",
    author: "u/LearnPlatform",
    time: "Promoted",
    type: "Ad",
    title: "Master Programming in 2025 - Get 50% Off All Courses",
    body: "Join 2M+ students learning to code. From beginner to advanced, we've got you covered.",
    image: "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?q=80&w=1000",
    upvotes: 0,
    comments: 0,
    shares: 0,
    tags: [],
    isAd: true,
  },
  {
    community: "r/cooking",
    author: "u/chefathome",
    time: "1 day ago",
    type: "Image",
    title: "Made my first sourdough bread from scratch! Recipe in comments",
    body: "",
    image: "https://images.unsplash.com/photo-1509440159596-0249088772ff?q=80&w=1000",
    upvotes: 3421,
    comments: 189,
    shares: 567,
    tags: ["Cooking", "Bread", "Recipe"],
  },
  {
    community: "r/cryptocurrency",
    author: "u/cryptotrader",
    time: "10 hours ago",
    type: "Discussion",
    title: "Bitcoin hits new all-time high: What's driving this bull run?",
    body: "BTC just crossed $100k. Let's discuss the factors behind this surge and where we think it's headed.",
    image: null,
    upvotes: 5678,
    comments: 1234,
    shares: 2345,
    tags: ["Bitcoin", "Crypto", "Trading"],
  },
  {
    community: "r/photography",
    author: "u/shutterbug",
    time: "14 hours ago",
    type: "Image",
    title: "Captured this perfect sunset moment at the Grand Canyon",
    body: "",
    image: "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?q=80&w=1000",
    upvotes: 6789,
    comments: 234,
    shares: 1123,
    tags: ["Photography", "Landscape", "Nature"],
  },
]

// Highlights data for Today's Highlights section
const highlightsData = [
  {
    type: "AMA",
    title: "I'm a NASA Engineer working on Mars missions - AMA!",
    community: "r/space",
    time: "Live Now",
    participants: 2847,
    icon: "fa-rocket",
  },
  {
    type: "Event",
    title: "Web3 Summit 2025: Join the discussion",
    community: "r/cryptocurrency",
    time: "Starting in 2 hours",
    participants: 1523,
    icon: "fa-calendar-check",
  },
  {
    type: "Breaking",
    title: "Major breakthrough in fusion energy announced",
    community: "r/science",
    time: "30 minutes ago",
    participants: 5621,
    icon: "fa-bolt",
  },
]

// Live talks data
const liveTalksData = [
  {
    title: "Tech Career Advice",
    host: "u/seniordev",
    listeners: 234,
    community: "r/programming",
  },
  {
    title: "Market Analysis Live",
    host: "u/wallstreetpro",
    listeners: 567,
    community: "r/investing",
  },
  {
    title: "Gaming News Roundup",
    host: "u/gamernews",
    community: "r/gaming",
  },
]

// Active discussions data
const activeDiscussionsData = [
  { title: "What's your unpopular tech opinion?", comments: 1234, time: "2h" },
  { title: "Best programming language for beginners?", comments: 892, time: "1h" },
  { title: "Is AI going to replace developers?", comments: 2341, time: "3h" },
  { title: "Favorite sci-fi movie of all time?", comments: 567, time: "4h" },
]

// Utility functions
function formatNumber(num) {
  if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + "M"
  } else if (num >= 1000) {
    return (num / 1000).toFixed(1) + "k"
  }
  return num.toString()
}

function getRandomInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min
}

function shuffleArray(array) {
  const newArray = [...array]
  for (let i = newArray.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[newArray[i], newArray[j]] = [newArray[j], newArray[i]]
  }
  return newArray
}

// Generate communities list
function generateCommunitiesList(container, communitiesArray, limit = 5) {
  const html = `
        <ul class="community-list">
            ${communitiesArray
              .slice(0, limit)
              .map(
                (community) => `
                <li>
                    <a href="#">
                        <div class="community-icon">${community.icon}</div>
                        ${community.name}
                        <span class="community-members">${community.members}</span>
                    </a>
                </li>
            `,
              )
              .join("")}
        </ul>
        ${limit < communitiesArray.length ? '<button class="btn btn-outline-primary sidebar-button mt-2">View All</button>' : ""}
    `
  container.innerHTML = html
}

// Generate categories list
function generateCategoriesList(container, categoriesArray) {
  const html = categoriesArray
    .map(
      (category) => `
        <a href="#" class="category-badge">
            <i class="fas ${category.icon}"></i> ${category.name}
        </a>
    `,
    )
    .join("")
  container.innerHTML = html
}

// Generate trending topics
function generateTrendingTopics(container, postsArray) {
  const topics = postsArray.slice(0, 5).map((post, index) => ({
    number: index + 1,
    title: post.title.substring(0, 50) + (post.title.length > 50 ? "..." : ""),
  }))

  const html = `
        <ul class="trending-topics">
            ${topics
              .map(
                (topic) => `
                <li>
                    <a href="#">
                        <span class="trending-number">${topic.number}</span>
                        ${topic.title}
                    </a>
                </li>
            `,
              )
              .join("")}
        </ul>
    `
  container.innerHTML = html
}

// Generate featured communities carousel
function generateFeaturedCommunities(container, communitiesArray) {
  const featured = communitiesArray.slice(0, 6)
  const html = featured
    .map(
      (community) => `
        <div class="featured-community-card">
            <div class="featured-community-icon">${community.icon}</div>
            <h4>${community.name}</h4>
            <p class="featured-community-members">${community.members} members</p>
            <p class="featured-community-category">${community.category}</p>
            <button class="btn btn-primary btn-sm">Join</button>
        </div>
    `,
    )
    .join("")
  container.innerHTML = html
}

// Generate highlights
function generateHighlights(container, highlightsArray) {
  const html = highlightsArray
    .map(
      (highlight) => `
        <div class="highlight-card">
            <div class="highlight-icon ${highlight.type.toLowerCase()}">
                <i class="fas ${highlight.icon}"></i>
            </div>
            <div class="highlight-content">
                <span class="highlight-type">${highlight.type}</span>
                <h4>${highlight.title}</h4>
                <div class="highlight-meta">
                    <span class="community-badge">${highlight.community}</span>
                    <span>•</span>
                    <span>${highlight.time}</span>
                    <span>•</span>
                    <span><i class="fas fa-users"></i> ${formatNumber(highlight.participants)}</span>
                </div>
                <button class="btn btn-outline-primary btn-sm mt-2">Join Now</button>
            </div>
        </div>
    `,
    )
    .join("")
  container.innerHTML = html
}

// Generate live talks
function generateLiveTalks(container, talksArray) {
  const html = `
        <ul class="live-talks-list">
            ${talksArray
              .map(
                (talk) => `
                <li>
                    <a href="#">
                        <div class="live-talk-header">
                            <span class="live-badge"><i class="fas fa-circle"></i> LIVE</span>
                            <span class="listeners-count"><i class="fas fa-headphones"></i> ${talk.listeners}</span>
                        </div>
                        <div class="live-talk-title">${talk.title}</div>
                        <div class="live-talk-meta">
                            <span>${talk.host}</span>
                            <span>•</span>
                            <span class="community-badge">${talk.community}</span>
                        </div>
                    </a>
                </li>
            `,
              )
              .join("")}
        </ul>
    `
  container.innerHTML = html
}

// Generate active discussions
function generateActiveDiscussions(container, discussionsArray) {
  const html = `
        <ul class="active-discussions-list">
            ${discussionsArray
              .map(
                (discussion) => `
                <li>
                    <a href="#">
                        <div class="discussion-title">${discussion.title}</div>
                        <div class="discussion-meta">
                            <span><i class="far fa-comment-alt"></i> ${formatNumber(discussion.comments)}</span>
                            <span>•</span>
                            <span>${discussion.time} ago</span>
                        </div>
                    </a>
                </li>
            `,
              )
              .join("")}
        </ul>
    `
  container.innerHTML = html
}

// Generate community spotlight
function generateCommunitySpotlight(container, community) {
  const html = `
        <div class="spotlight-content">
            <div class="spotlight-icon">${community.icon}</div>
            <h4>${community.name}</h4>
            <p class="spotlight-members">${community.members} members</p>
            <p class="spotlight-description">A thriving community for ${community.category.toLowerCase()} enthusiasts. Join discussions, share knowledge, and connect with like-minded people.</p>
            <button class="btn btn-primary sidebar-button">Join Community</button>
        </div>
    `
  container.innerHTML = html
}

// Generate post card
function generatePostCard(post) {
  const netVotes = post.upvotes
  const voteClass = netVotes > 0 ? "positive" : netVotes < 0 ? "negative" : ""

  // Handle advertisement posts
  if (post.isAd) {
    return `
            <div class="post-card ad-post">
                <div class="ad-label">Sponsored</div>
                <div class="post-content">
                    <div class="post-main">
                        <div class="post-meta">
                            <span class="community-badge">${post.community}</span>
                            <span>•</span>
                            <span>${post.time}</span>
                        </div>
                        <h2 class="post-title">
                            <a href="#">${post.title}</a>
                        </h2>
                        ${post.body ? `<div class="post-body">${post.body}</div>` : ""}
                        ${
                          post.image
                            ? `
                            <div class="post-body">
                                <img src="${post.image}" alt="${post.title}" class="post-image">
                            </div>
                        `
                            : ""
                        }
                        <div class="post-actions">
                            <button class="btn btn-primary">Learn More</button>
                        </div>
                    </div>
                </div>
            </div>
        `
  }

  // Handle announcement posts
  const pinnedBadge = post.isPinned ? '<span class="pinned-badge"><i class="fas fa-thumbtack"></i> Pinned</span>' : ""

  return `
        <div class="post-card ${post.isPinned ? "pinned-post" : ""}">
            <div class="post-content">
                <div class="post-main">
                    <div class="post-meta">
                        ${pinnedBadge}
                        <a href="#" class="community-badge">${post.community}</a>
                        <span>•</span>
                        <span>Posted by
                            <a href="#" class="community-badge" style="font-weight: 400;">${post.author}</a>
                            <span>${post.time}</span>
                        </span>
                        <span class="post-type-indicator ${post.type.toLowerCase()}">${post.type}</span>
                    </div>
                    ${
                      post.link
                        ? `
                        <div class="post-link">
                            <i class="fas fa-link"></i>
                            <a href="#">${post.link}</a>
                        </div>
                    `
                        : ""
                    }
                    <h2 class="post-title">
                        <a href="#">${post.title}</a>
                    </h2>
                    ${post.body ? `<div class="post-body">${post.body}</div>` : ""}
                    ${
                      post.image
                        ? `
                        <div class="post-body">
                            <img src="${post.image}" alt="${post.title}" class="post-image">
                        </div>
                    `
                        : ""
                    }
                    ${
                      post.poll
                        ? `
                        <div class="poll-container">
                            ${post.poll.options
                              .map(
                                (option) => `
                                <div class="poll-option">
                                    <div class="poll-option-bar" style="width: ${option.percentage}%"></div>
                                    <div class="poll-option-content">
                                        <span class="poll-option-text">${option.text}</span>
                                        <span class="poll-option-percentage">${option.percentage}%</span>
                                    </div>
                                    <div class="poll-option-votes">${formatNumber(option.votes)} votes</div>
                                </div>
                            `,
                              )
                              .join("")}
                            <div class="poll-footer">
                                <span>${formatNumber(post.poll.totalVotes)} total votes</span>
                                <span>•</span>
                                <span>${post.poll.endsIn}</span>
                            </div>
                        </div>
                    `
                        : ""
                    }
                    ${
                      post.tags
                        ? `
                        <div class="tags-container">
                            ${post.tags.map((tag) => `<span class="tag-badge">${tag}</span>`).join("")}
                        </div>
                    `
                        : ""
                    }
                    <div class="post-actions">
                        <div class="voting-buttons">
                            <button class="vote-button upvote" onclick="handleVote(this, 'upvote')">
                                <i class="fas fa-arrow-up"></i>
                            </button>
                            <span class="vote-count ${voteClass}">${formatNumber(netVotes)}</span>
                            <button class="vote-button downvote" onclick="handleVote(this, 'downvote')">
                                <i class="fas fa-arrow-down"></i>
                            </button>
                        </div>
                        <div class="post-stats">
                            <button class="action-button">
                                <i class="far fa-comment-alt"></i>
                                <span>${post.comments} Comments</span>
                            </button>
                            <button class="action-button">
                                <i class="fas fa-share"></i>
                                <span>Share</span>
                                ${post.shares > 0 ? `<span class="share-count">• ${formatNumber(post.shares)}</span>` : ""}
                            </button>
                            <button class="action-button">
                                <i class="far fa-bookmark"></i>
                                <span>Save</span>
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `
}

// Handle vote functionality
function handleVote(button, type) {
  const votingButtons = button.closest(".voting-buttons")
  const upvoteBtn = votingButtons.querySelector(".upvote")
  const downvoteBtn = votingButtons.querySelector(".downvote")
  const voteCountSpan = votingButtons.querySelector(".vote-count")

  let currentCount = Number.parseInt(voteCountSpan.textContent.replace(/[^\d-]/g, "")) || 0
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

  voteCountSpan.textContent = formatNumber(currentCount)
}

// Initialize page
function initializePage() {
  // Shuffle data for variety
  const shuffledCommunities = shuffleArray(communities)
  const shuffledPosts = shuffleArray(postsData)

  // Generate featured communities
  const featuredCommunitiesContainer = document.getElementById("featured-communities")
  if (featuredCommunitiesContainer) {
    generateFeaturedCommunities(featuredCommunitiesContainer, shuffledCommunities)
  }

  // Generate highlights
  const highlightsContainer = document.getElementById("highlights-grid")
  if (highlightsContainer) {
    generateHighlights(highlightsContainer, highlightsData)
  }

  // Generate live talks
  const liveTalksContainer = document.getElementById("live-talks")
  if (liveTalksContainer) {
    generateLiveTalks(liveTalksContainer, liveTalksData)
  }

  // Generate active discussions
  const activeDiscussionsContainer = document.getElementById("active-discussions")
  if (activeDiscussionsContainer) {
    generateActiveDiscussions(activeDiscussionsContainer, activeDiscussionsData)
  }

  // Generate community spotlight
  const spotlightContainer = document.getElementById("community-spotlight")
  if (spotlightContainer) {
    const randomCommunity = shuffledCommunities[Math.floor(Math.random() * shuffledCommunities.length)]
    generateCommunitySpotlight(spotlightContainer, randomCommunity)
  }

  // Generate your communities
  const yourCommunitiesContainer = document.getElementById("your-communities")
  if (yourCommunitiesContainer) {
    generateCommunitiesList(yourCommunitiesContainer, shuffledCommunities.slice(0, 5), 5)
  }

  // Generate top communities
  const topCommunitiesContainer = document.getElementById("top-communities")
  if (topCommunitiesContainer) {
    generateCommunitiesList(topCommunitiesContainer, shuffledCommunities.slice(5, 10), 5)
  }

  // Generate categories
  const categoriesContainer = document.getElementById("categories-list")
  if (categoriesContainer) {
    generateCategoriesList(categoriesContainer, categories)
  }

  // Generate trending topics
  const trendingContainer = document.getElementById("trending-topics")
  if (trendingContainer) {
    generateTrendingTopics(trendingContainer, shuffledPosts)
  }

  // Generate posts
  const postsContainer = document.getElementById("posts-container")
  if (postsContainer) {
    postsContainer.innerHTML = shuffledPosts.map((post) => generatePostCard(post)).join("")
  }
}

// Document ready event listener
document.addEventListener("DOMContentLoaded", () => {
  initializePage()

  // Filter tabs functionality
  document.querySelectorAll(".filter-tab").forEach((tab) => {
    tab.addEventListener("click", function () {
      document.querySelectorAll(".filter-tab").forEach((t) => t.classList.remove("active"))
      this.classList.add("active")

      const filter = this.getAttribute("data-filter")
      console.log(`[v0] Filtering by: ${filter}`)

      // Re-shuffle and regenerate posts
      const shuffledPosts = shuffleArray(postsData)
      const postsContainer = document.getElementById("posts-container")
      if (postsContainer) {
        postsContainer.innerHTML = shuffledPosts.map((post) => generatePostCard(post)).join("")
      }
    })
  })

  // Post type selector in modal
  document.querySelectorAll(".post-type-option").forEach((option) => {
    option.addEventListener("click", function () {
      document.querySelectorAll(".post-type-option").forEach((opt) => opt.classList.remove("active"))
      this.classList.add("active")

      const postType = this.getAttribute("data-post-type")

      document
        .querySelectorAll(
          "#text-post-content, #image-post-content, #link-post-content, #video-post-content, #poll-post-content",
        )
        .forEach((container) => {
          container.style.display = "none"
        })

      document.getElementById(`${postType}-post-content`).style.display = "block"
    })
  })

  // Handle file upload button clicks
  document.querySelectorAll(".file-upload-container .btn").forEach((button) => {
    button.addEventListener("click", function (e) {
      e.preventDefault()
      const container = this.closest(".file-upload-container")
      const fileInput = container.querySelector('input[type="file"]')
      fileInput.click()
    })
  })

  // Initialize create post modal with post type
  const createPostModal = document.getElementById("createPostModal")
  if (createPostModal) {
    createPostModal.addEventListener("show.bs.modal", (event) => {
      const button = event.relatedTarget
      if (button && button.hasAttribute("data-post-type")) {
        const postType = button.getAttribute("data-post-type")

        document.querySelectorAll(".post-type-option").forEach((opt) => opt.classList.remove("active"))
        const targetOption = document.querySelector(`.post-type-option[data-post-type="${postType}"]`)
        if (targetOption) {
          targetOption.classList.add("active")
        }

        document
          .querySelectorAll(
            "#text-post-content, #image-post-content, #link-post-content, #video-post-content, #poll-post-content",
          )
          .forEach((container) => {
            container.style.display = "none"
          })

        const targetContent = document.getElementById(`${postType}-post-content`)
        if (targetContent) {
          targetContent.style.display = "block"
        }
      }
    })
  }
})
