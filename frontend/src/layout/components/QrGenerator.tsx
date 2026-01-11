import React, { useState, useEffect } from "react";
import QRCode from "react-qr-code";
import "../styles/QrGenerator.css";

interface QrGeneratorProperties {
  value: string;
}

const QrGenerator: React.FC<QrGeneratorProperties> = ({ value }) => {
  const [qrSize, setQrSize] = useState(250);

  useEffect(() => {
    const handleResize = () => {
      setQrSize(window.innerWidth < 768 ? 250 : 500);
    };

    handleResize();
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  return (
    <div className="flex justify-center items-center p-4">
      <QRCode
        className="p-7 bg-main-text rounded-3xl border-15 border-teritary-dark"
        value={value}
        size={qrSize}
        bgColor="#BCBCBD"
      />
    </div>
  );
};

export default QrGenerator;
