// wwwroot/js/quillInterop.js
export async function initializeQuill(elementId) {
    // load Quill core
    await loadScript('https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.min.js');
    // load the table plugin
    await loadScript('https://cdn.jsdelivr.net/npm/quill-table-better@1.0.7/dist/quill-table-better.js');
  
    // register and initialize
    Quill.register({ 'modules/table-better': QuillTableBetter }, true);
    window.quill = new Quill(elementId, {
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
            [{ list:'ordered' },{ list:'bullet' }],
            ['link','image','video'],
            ['table-better'],
            ['clean']
          ],
          handlers: {
            'table-better': () => {
              window.quill.getModule('table-better').insertTable(3,3);
            }
          }
        }
      },
      placeholder: 'Type here…'
    });
  }
  
  function loadScript(src) {
    return new Promise((resolve, reject) => {
      const s = document.createElement('script');
      s.src = src;
      s.onload = resolve;
      s.onerror = reject;
      document.head.appendChild(s);
    });
  }
  