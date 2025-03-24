window.addEventListener("load", () => {
    const qrCodeData = document.getElementById("qrCodeData").value;
    new QRCode(document.getElementById("qrCode"), {
      text: qrCodeData,
      width: 150,
      height: 150
    });
  });
  