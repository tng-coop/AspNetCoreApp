# ASP.NET 9 Blazor WebApp Developer Machine Memo

**Location**  
`yasu@ubuntu:~/co/AspNetCoreApp/BlazorWebApp (master)`

---

## Apache Virtual Host: aspnet.lan

Configuration file:  
```
/etc/apache2/sites-available/aspnet.test.conf
```

### 1. Secure Frontend (Port 443 → Backend 5010)

```apache
<VirtualHost *:443>
    ServerName aspnet.lan

    # SSL certificates
    SSLEngine on
    SSLCertificateFile      /srv/shared/aspnet/cert/aspnet.lan.crt
    SSLCertificateKeyFile   /srv/shared/aspnet/cert/aspnet.lan.key
    SSLCertificateChainFile /srv/shared/aspnet/cert/aspnet.lan-ca.crt

    # Preserve host header and enable SSL proxy
    ProxyPreserveHost On
    SSLProxyEngine   on
    ProxyRequests    Off

    # Proxy all traffic to Kestrel on port 5010
    ProxyPass        "/" "https://aspnet.lan:5010/"
    ProxyPassReverse "/" "https://aspnet.lan:5010/"
</VirtualHost>
```

- **ServerName**: `aspnet.lan`  
- **Backend**: Kestrel listening on `https://aspnet.lan:5010`  
- **Purpose**: Terminate SSL at Apache, forward encrypted traffic to the ASP.NET Core app.

---

### 2. HTTP Frontend (Port 80)

```apache
<VirtualHost *:80>
    ServerName    aspnet.lan
    ServerAlias   localhost

    # Proxy settings
    ProxyPreserveHost On
    SSLProxyEngine   Off
    ProxyRequests    Off

    # 1) Proxy /cert endpoints to Kestrel on port 5000
    ProxyPass        "/cert"  "http://127.0.0.1:5000/cert"
    ProxyPassReverse "/cert"  "http://127.0.0.1:5000/cert"
    ProxyPass        "/cert/" "http://127.0.0.1:5000/cert/"
    ProxyPassReverse "/cert/" "http://127.0.0.1:5000/cert/"

    # 2) (Optional) Redirect all other HTTP → HTTPS
    #
    # Uncomment to force HTTPS for everything except /cert
    #
    # RewriteEngine On
    # RewriteCond %{REQUEST_URI} !^/cert(/|$)
    # RewriteRule ^ https://%{HTTP_HOST}%{REQUEST_URI} [L,R=302]
</VirtualHost>
```

- **ServerName**: `aspnet.lan` (also serves `localhost`)  
- **Endpoints**:
  1. **/cert** → forwarded to Kestrel on port 5000 (no SSL)  
  2. **Everything else** → (optional) redirect to HTTPS on same host  
- **Purpose**: Allow unencrypted access to certificate endpoints, optionally upgrade all other HTTP to HTTPS.

---

## Enabling the Site

```bash
sudo a2ensite aspnet.test.conf
sudo systemctl reload apache2
```

- Ensure SSL module and proxy modules are enabled:
  ```bash
  sudo a2enmod ssl proxy proxy_http proxy_balancer lbmethod_byrequests rewrite
  sudo systemctl restart apache2
  ```

---

## Notes

- Make sure your DNS or `/etc/hosts` maps `aspnet.lan` to your dev machine’s IP.
- Kestrel must be running on ports **5000** (for `/cert`) and **5010** (for the main app).
- Adjust firewall rules to allow incoming ports **80** and **443**.
