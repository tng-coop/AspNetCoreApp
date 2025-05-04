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
          ['table-better'],  // table icon
          ['clean']
        ],
        handlers: {
          'table-better': function() {
            // Insert a 3×3 table
            quill.getModule('table-better').insertTable(3, 3);
          }
        }
      }
    },
    placeholder: 'Type here...'
  });
}

export function setContents(deltaJson) {
  try {
    quill.setContents(JSON.parse(deltaJson));
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
