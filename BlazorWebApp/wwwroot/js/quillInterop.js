// quillInterop.js
export async function initializeQuill(selector) {
  const load = src => new Promise((r, e) => {
    const s = document.createElement('script');
    s.src = src; s.onload = r; s.onerror = e;
    document.head.appendChild(s);
  });
  await load('https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.min.js');
  Quill.register({'modules/table-better': QuillTableBetter}, true);
  window.quill = new Quill(selector, {
    theme: 'snow',
    modules: {
      table: false,
      'table-better': {},
      keyboard: { bindings: QuillTableBetter.keyboardBindings },
      toolbar: {
        container: [
          [{ header: [1,2,3,false] }],
          ['bold','italic','underline','strike'],
          ['blockquote','code-block'],
          [{ list:'ordered' }, { list:'bullet' }],
          ['link','image','video'],
          ['table-better'],
          ['clean']
        ]
      }
    },
    placeholder: 'Type here…'
  });
}

export function getDeltaJson() {
  return JSON.stringify(window.quill.getContents());
}

export function getHtml() {
  return window.quill.root.innerHTML;
}
