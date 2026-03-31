async function renderMarkdown({ mdPath, title }) {
  const res = await fetch(mdPath, { cache: 'no-cache' });
  if (!res.ok) {
    throw new Error(`Failed to load ${mdPath}: ${res.status}`);
  }
  const md = await res.text();

  // Strip YAML front matter if present
  const cleaned = md.replace(/^---\s*\n[\s\S]*?\n---\s*\n/, '');

  const html = marked.parse(cleaned, {
    gfm: true,
    breaks: false,
  });

  document.title = title;
  const el = document.getElementById('content');
  el.innerHTML = html;

  // Make external links open in new tab
  for (const a of el.querySelectorAll('a[href]')) {
    const href = a.getAttribute('href') || '';
    if (href.startsWith('http://') || href.startsWith('https://')) {
      a.setAttribute('target', '_blank');
      a.setAttribute('rel', 'noopener noreferrer');
    }
  }
}
