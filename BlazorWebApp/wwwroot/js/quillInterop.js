// wwwroot/js/quillInterop.js
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
  // load local scripts only
  await loadScript('/lib/quill/dist/quill.js');
  await loadScript('/lib/quill-table-better/dist/quill-table-better.js');
  Quill.register({ 'modules/table-better': QuillTableBetter }, true);

  quill = new Quill(selector, {
    theme: 'snow',
    modules: {
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
          [{ color: [] }, { background: [] }],
          ['clean']
        ],
        handlers: {
          image: () => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = 'image/*';
            input.click();
            input.onchange = async () => {
              const file = input.files[0];
              const form = new FormData();
              form.append('file', file);
              const res = await fetch('/api/images/upload', { method: 'POST', body: form });
              const { url } = await res.json();
                            // use the returned URL’s path (preserves extension)
              const relativePath = url.startsWith('http') ? new URL(url).pathname : url;
              const range = quill.getSelection(true);
              quill.insertEmbed(range.index, 'image', relativePath);
              quill.setSelection(range.index + 1);
            };
          },
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
    quill.updateContents(delta, Quill.sources.USER);
    const length = delta.ops.reduce(
      (sum, op) => (typeof op.insert === 'string' ? sum + op.insert.length : sum + 1),
      0
    );
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

export function setHtml(html) {
  quill.clipboard.dangerouslyPasteHtml(html);
}
