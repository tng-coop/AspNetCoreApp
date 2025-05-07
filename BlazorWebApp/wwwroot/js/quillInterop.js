let quill;

const loadScript = src =>
  new Promise((res, rej) => {
    const s = document.createElement('script');
    s.src = src;
    s.onload = res;
    s.onerror = rej;
    document.head.appendChild(s);
  });

export async function initializeQuill(selector) {
  // inject Quill Snow stylesheet
  const snowCss = document.createElement('link');
  snowCss.rel = 'stylesheet';
  snowCss.href = 'https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.snow.css';
  document.head.appendChild(snowCss);

  // inject table-better stylesheet
  const tableCss = document.createElement('link');
  tableCss.rel = 'stylesheet';
  tableCss.href = 'https://cdn.jsdelivr.net/npm/quill-table-better@1.0.7/dist/quill-table-better.css';
  document.head.appendChild(tableCss);

  // Load Quill core and table-better plugin
  await loadScript('https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.min.js');
  await loadScript('https://cdn.jsdelivr.net/npm/quill-table-better@1.0.7/dist/quill-table-better.js');

  // Register the table-better plugin
  Quill.register({'modules/table-better': QuillTableBetter}, true);

  // Initialize Quill with toolbar and table handler
  quill = new Quill(selector, {
    theme: 'snow',
    modules: {
      table: false,
      'table-better': {},
      keyboard: { bindings: QuillTableBetter.keyboardBindings },
      toolbar: {
        container: [
          [{ header: [1, 2, 3, false] }],
          ['bold', 'italic', 'underline', 'strike'],
          ['blockquote', 'code-block'],
          [{ list: 'ordered' }, { list: 'bullet' }],
          ['link', 'image', 'video'],
          ['table-better'],
          ['clean']
        ],
        handlers: {
          'table-better': () => quill.getModule('table-better').insertTable(3, 3)
        }
      }
    },
    placeholder: 'Type here...'
  });
}

export function setContents(deltaJson) {
  try {
    const delta = JSON.parse(deltaJson);
    // use updateContents to properly re-create table blots
    quill.updateContents(delta, Quill.sources.USER);
    // move cursor to end
    const length = delta.ops.reduce((sum, op) => {
      if (typeof op.insert === 'string') return sum + op.insert.length;
      return sum + 1;
    }, 0);
    quill.setSelection(length, Quill.sources.SILENT);
    quill.scrollIntoView();
  } catch (err) {
    console.warn('Failed to parse or apply delta JSON', err);
  }
}

export function getDeltaJson() {
  return JSON.stringify(quill.getContents());
}

export function getHtml() {
  return quill.root.innerHTML;
}

// optional: restore full HTML when Delta isn't enough
export function setHtml(html) {
  quill.clipboard.dangerouslyPasteHtml(html);
}
