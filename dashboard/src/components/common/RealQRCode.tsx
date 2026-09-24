import React, { useEffect, useRef, useState } from 'react';
import QRCode from 'qrcode';
import { Download, Check, Copy } from 'lucide-react';

interface RealQRCodeProps {
  value: string;
  size?: number;
  className?: string;
  showActions?: boolean;
  downloadFilename?: string;
}

export const RealQRCode: React.FC<RealQRCodeProps> = ({
  value,
  size = 140,
  className = '',
  showActions = false,
  downloadFilename = 'certificate_qr.png',
}) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const [dataUrl, setDataUrl] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (!canvasRef.current || !value) return;

    QRCode.toCanvas(
      canvasRef.current,
      value,
      {
        width: size * 2, // 2x for sharp retina rendering
        margin: 2,
        color: {
          dark: '#0F172A', // Slate 900 high-contrast
          light: '#FFFFFF',
        },
        errorCorrectionLevel: 'M',
      },
      (error) => {
        if (error) {
          console.error('[RealQRCode] Error rendering QR code:', error);
          return;
        }
        if (canvasRef.current) {
          setDataUrl(canvasRef.current.toDataURL('image/png'));
        }
      }
    );
  }, [value, size]);

  const handleDownload = () => {
    if (!dataUrl) return;
    const a = document.createElement('a');
    a.href = dataUrl;
    a.download = downloadFilename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  };

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(value);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy QR value:', err);
    }
  };

  return (
    <div className={`flex flex-col items-center ${className}`}>
      <div
        className="p-2.5 rounded-xl bg-white border border-slate-200 shadow-sm relative group overflow-hidden"
        style={{ width: size + 20, height: size + 20 }}
      >
        <canvas
          ref={canvasRef}
          style={{ width: size, height: size }}
          className="rounded-lg block mx-auto"
        />

        {showActions && (
          <div className="absolute inset-0 bg-slate-950/80 backdrop-blur-[2px] opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2 rounded-xl">
            <button
              onClick={handleDownload}
              title="Download QR Code PNG"
              className="p-2 rounded-lg bg-suraksha-amber text-slate-950 hover:bg-amber-400 transition"
            >
              <Download className="w-4 h-4" />
            </button>
            <button
              onClick={handleCopy}
              title="Copy Verification Link"
              className="p-2 rounded-lg bg-white/20 text-white hover:bg-white/30 transition"
            >
              {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
            </button>
          </div>
        )}
      </div>

      {showActions && (
        <div className="mt-2 flex items-center gap-2">
          <button
            onClick={handleDownload}
            className="flex items-center gap-1 text-[11px] font-semibold text-suraksha-subtext hover:text-suraksha-amber transition"
          >
            <Download className="w-3 h-3" />
            <span>Download PNG</span>
          </button>
          <span className="text-slate-600 text-xs">•</span>
          <button
            onClick={handleCopy}
            className="flex items-center gap-1 text-[11px] font-semibold text-suraksha-subtext hover:text-suraksha-heading transition"
          >
            {copied ? <Check className="w-3 h-3 text-emerald-400" /> : <Copy className="w-3 h-3" />}
            <span>{copied ? 'Copied Link' : 'Copy Link'}</span>
          </button>
        </div>
      )}
    </div>
  );
};
