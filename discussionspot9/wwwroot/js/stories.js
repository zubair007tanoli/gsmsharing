(function(){
  function progressBar(count){
    const root = document.getElementById('storiesProgress');
    root.innerHTML = '';
    const bars = [];
    for(let i=0;i<count;i++){
      const bar = document.createElement('div');
      bar.className = 'bar';
      const fill = document.createElement('span');
      bar.appendChild(fill);
      bars.push(fill);
      root.appendChild(bar);
    }
    return bars;
  }

  function renderDots(count, current){
    const root = document.getElementById('storiesDots');
    if(!root) return [];
    root.innerHTML = '';
    const dots = [];
    for(let i=0;i<count;i++){
      const dot = document.createElement('button');
      dot.className = 'dot' + (i===current ? ' active' : '');
      dot.type = 'button';
      dot.setAttribute('aria-label', 'Slide '+(i+1));
      dot.addEventListener('click', ()=>{ storiesPlayer.go(i); });
      root.appendChild(dot);
      dots.push(dot);
    }
    return dots;
  }

  async function fetchSlides(slug){
    try{
      const res = await fetch(`/api/stories/${slug}/slides`, { headers: { 'Accept': 'application/json' } });
      if(!res.ok) throw new Error('no json');
      const data = await res.json();
      if(data && data.success && Array.isArray(data.slides) && data.slides.length){
        return { title: data.title || '', pageUrl: data.pageUrl || '', slides: data.slides };
      }
      throw new Error('empty');
    }catch(e){
      // Fallback loads classic viewer page in iframe
      return { slides:[{type:'iframe', src:'/stories/viewer/'+slug}] };
    }
  }

  const storiesPlayer = {
    currentIndex: 0,
    items: [],
    bars: [],
    dots: [],
    timer: null,
    durationMs: 8000,
    paused: false,
    dragging: false,
    open(items){
      this.items = items;
      this.currentIndex = 0;
      this.bars = progressBar(items.length);
      const modal = document.getElementById('storiesPlayerModal');
      modal.classList.add('active');
      this.render(true);
      this.play();
    },
    close(){
      clearInterval(this.timer);
      document.getElementById('storiesPlayerModal').classList.remove('active');
      document.getElementById('storiesStage').innerHTML='';
      document.getElementById('storiesProgress').innerHTML='';
    },
    render(initial=false){
      const stage = document.getElementById('storiesStage');
      stage.innerHTML='';
      const item = this.items[this.currentIndex];
      const titleEl = document.getElementById('storiesTitle');
      const openEl = document.getElementById('storiesOpenPage');
      const openElBottom = document.getElementById('storiesOpenPageBottom');
      titleEl.textContent = item.title || '';
      const pageUrl = item.pageUrl || '#';
      if(openEl){ openEl.href = pageUrl; }
      if(openElBottom){ openElBottom.href = pageUrl; }
      if(item.type==='iframe'){
        const iframe = document.createElement('iframe');
        iframe.src = item.src;
        iframe.style.border='0';
        iframe.width='100%';
        iframe.height='100%';
        stage.appendChild(iframe);
      } else if (item.type==='image') {
        const img = document.createElement('img');
        img.src = item.src;
        img.alt = item.caption || titleEl.textContent || 'Story image';
        img.style.width='100%';
        img.style.height='100%';
        img.style.objectFit='cover';
        stage.appendChild(img);
      } else if (item.type==='video') {
        const video = document.createElement('video');
        video.src = item.src;
        if(item.poster){ video.poster = item.poster; }
        video.autoplay = true; video.muted = true; video.playsInline = true; video.loop = true;
        video.style.width='100%';
        video.style.height='100%';
        video.style.objectFit='cover';
        stage.appendChild(video);
      }
      this.updateBars();
      if(initial){ this.attachGestures(stage); }
      this.dots = renderDots(this.items.length, this.currentIndex);
    },
    updateBars(){
      for(let i=0;i<this.bars.length;i++){
        this.bars[i].style.width = i < this.currentIndex ? '100%' : '0%';
      }
    },
    play(){
      const start = Date.now();
      clearInterval(this.timer);
      const self = this;
      this.timer = setInterval(function(){
        if(self.paused) return;
        const elapsed = Date.now()-start;
        const dur = self.items[self.currentIndex]?.duration || self.durationMs;
        const pct = Math.min(100, Math.round((elapsed/dur)*100));
        if(self.bars[self.currentIndex]){
          self.bars[self.currentIndex].style.width = pct+'%';
        }
        if(elapsed >= dur){
          clearInterval(self.timer);
          self.next();
        }
      }, 50);
    },
    next(){
      if(this.currentIndex < this.items.length-1){
        this.currentIndex++;
        this.render();
        this.play();
      } else {
        this.close();
      }
    },
    prev(){
      if(this.currentIndex>0){
        this.currentIndex--;
        this.render();
        this.play();
      }
    },
    go(index){
      if(index<0 || index>=this.items.length) return;
      this.currentIndex = index;
      this.render();
      this.play();
    },
    pause(){ this.paused = true; },
    resume(){ this.paused = false; },
    togglePause(){ this.paused = !this.paused; },
    attachGestures(stage){
      let startX = 0, endX = 0;
      stage.addEventListener('mouseenter', ()=>{ this.pause(); });
      stage.addEventListener('mouseleave', ()=>{ this.resume(); });
      stage.addEventListener('mousedown', ()=>{ this.pause(); });
      stage.addEventListener('mouseup', ()=>{ this.resume(); });
      stage.addEventListener('touchstart', (e)=>{ this.pause(); startX = e.touches[0].clientX; }, {passive:true});
      stage.addEventListener('touchmove', (e)=>{ endX = e.touches[0].clientX; }, {passive:true});
      stage.addEventListener('touchend', ()=>{
        const delta = (endX - startX);
        if(Math.abs(delta) > 40){ if(delta < 0) this.next(); else this.prev(); }
        this.resume();
      });
    }
  };

  const storiesStrip = {
    async init(){
      const track = document.getElementById('storiesTrack');
      if(track){
        track.addEventListener('click', async function(e){
          const tile = e.target.closest('.story-tile');
          if(!tile) return;
          const slug = tile.getAttribute('data-story-slug');
          if(!slug) return;
          // Always navigate to AMP view for a consistent experience
          const ampUrl = '/stories/amp/'+slug;
          if(e.ctrlKey || e.metaKey){
            window.open(ampUrl, '_blank', 'noopener');
          } else {
            window.location.href = ampUrl;
          }
        });
      }

      // Reveal animations
      const observer = new IntersectionObserver((entries)=>{
        entries.forEach(entry=>{
          if(entry.isIntersecting){ entry.target.classList.add('in'); }
        });
      }, {threshold: 0.12});

      document.querySelectorAll('.reveal').forEach(el=>observer.observe(el));

      document.addEventListener('keydown', function(ev){
        if(!document.getElementById('storiesPlayerModal').classList.contains('active')) return;
        if(ev.key==='ArrowRight') storiesPlayer.next();
        if(ev.key==='ArrowLeft') storiesPlayer.prev();
        if(ev.key===' ') { ev.preventDefault(); storiesPlayer.togglePause(); }
        if(ev.key==='Escape') storiesPlayer.close();
      });
    },
    scrollBy(n){
      const track = document.getElementById('storiesTrack');
      if(!track) return;
      const child = track.querySelector('.story-tile');
      const step = child ? (child.getBoundingClientRect().width+12) : 120;
      track.scrollBy({left: n*step, behavior:'smooth'});
    }
  };

  window.storiesStrip = storiesStrip;
  window.storiesPlayer = storiesPlayer;
  document.addEventListener('DOMContentLoaded', function(){ storiesStrip.init(); });
})();


