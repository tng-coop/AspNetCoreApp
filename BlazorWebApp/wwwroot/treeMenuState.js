window.treeMenuState = {
    save: function(list) {
        const json = JSON.stringify(list || []);
        localStorage.setItem('treeMenuExpanded', json);
        document.cookie = 'treeMenuExpanded=' + encodeURIComponent(json) + '; path=/';
    },
    load: function() {
        return localStorage.getItem('treeMenuExpanded') || '';
    }
};
