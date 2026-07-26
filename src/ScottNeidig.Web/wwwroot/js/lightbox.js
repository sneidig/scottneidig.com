// Click-to-enlarge for images in a blog post body. Loaded only on post pages, so the rest of
// the site stays script-free. Each .prose image is wrapped in a button (keyboard-operable) with
// a magnifier badge; clicking opens a full-screen overlay. Esc or a click closes it.
(function () {
  var images = document.querySelectorAll('.prose img');
  if (!images.length) return;

  var overlay = document.createElement('div');
  overlay.className = 'lightbox';
  overlay.setAttribute('hidden', '');
  overlay.innerHTML = '<img class="lightbox__img" alt="">';
  var overlayImg = overlay.querySelector('.lightbox__img');
  document.body.appendChild(overlay);

  function open(src, alt) {
    overlayImg.src = src;
    overlayImg.alt = alt || '';
    overlay.removeAttribute('hidden');
    document.body.classList.add('lightbox-open');
  }

  function close() {
    overlay.setAttribute('hidden', '');
    overlayImg.removeAttribute('src');
    document.body.classList.remove('lightbox-open');
  }

  overlay.addEventListener('click', close);
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && !overlay.hasAttribute('hidden')) close();
  });

  var badge =
    '<span class="zoomable__badge" aria-hidden="true">' +
    '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" ' +
    'stroke-width="2" stroke-linecap="round"><circle cx="11" cy="11" r="7"></circle>' +
    '<line x1="21" y1="21" x2="16.5" y2="16.5"></line></svg></span>';

  images.forEach(function (img) {
    var button = document.createElement('button');
    button.type = 'button';
    button.className = 'zoomable';
    button.setAttribute('aria-label', 'View larger image');

    img.parentNode.insertBefore(button, img);
    button.appendChild(img);
    button.insertAdjacentHTML('beforeend', badge);

    button.addEventListener('click', function () {
      open(img.src, img.alt);
    });
  });
})();
