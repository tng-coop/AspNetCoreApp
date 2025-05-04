// wwwroot/js/quillInterop.js

/**
 * Dynamically load a script by URL.
 * @param {string} src 
 * @returns {Promise<void>}
 */
function loadScript(src) {
    return new Promise((resolve, reject) => {
      const s = document.createElement('script');
      s.src = src;
      s.onload = () => resolve();
      s.onerror = () => reject(new Error(`Failed to load ${src}`));
      document.head.appendChild(s);
    });
  }
  
  /**
   * Initialize Quill with the table-better plugin on the given element.
   * @param {string} elementId  CSS selector (e.g. "#editor")
   */
  export async function initializeQuill(elementId) {
    // 1. Load Quill core
    await loadScript('https://cdn.jsdelivr.net/npm/quill@2.0.3/dist/quill.min.js');
    // 2. Load the table-better plugin
    await loadScript('https://cdn.jsdelivr.net/npm/quill-table-better@1.0.7/dist/quill-table-better.js');
  
    // 3. Register and instantiate
    Quill.register({ 'modules/table-better': QuillTableBetter }, true);
    window.quill = new Quill(elementId, {
      theme: 'snow',
      modules: {
        table: false,
        'table-better': {},
        keyboard: { bindings: QuillTableBetter.keyboardBindings },
        toolbar: {
          container: [
            [{ header: [1, 2, 3, false] }],
            ['bold','italic','underline','strike'],
            ['blockquote','code-block'],
            [{ list: 'ordered' }, { list: 'bullet' }],
            ['link','image','video'],
            ['table-better'],
            ['clean']
          ],
          handlers: {
            'table-better': () => {
              window.quill.getModule('table-better').insertTable(3, 3);
            }
          }
        }
      },
      placeholder: 'Type here…'
    });
  }
  
  /**
   * Calculate the total number of bytes you’d need to save:
   *  • HTML markup + any inline base64 data-URIs
   *  • PLUS each <img> whose src is an external URL (via Content-Length header)
   * @returns {Promise<number>}
   */
  export async function getPayloadSize() {
    const html = window.quill.root.innerHTML;
    let total = new Blob([html]).size;
  
    // For each external image, HEAD-request its Content-Length
    const imgs = Array.from(window.quill.root.querySelectorAll('img'));
    for (let img of imgs) {
      const src = img.src;
      if (!src.startsWith('data:')) {
        try {
          const resp = await fetch(src, { method: 'HEAD' });
          const len  = resp.headers.get('Content-Length');
          if (len) total += parseInt(len, 10);
        } catch (_) {
          // ignore failures
        }
      }
    }
  
    return total;
  }
  