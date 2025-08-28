window.isRunningInWp = () => !!(window.wpApiSettings?.root || document.querySelector('meta[name="x-wp-nonce"]'));
