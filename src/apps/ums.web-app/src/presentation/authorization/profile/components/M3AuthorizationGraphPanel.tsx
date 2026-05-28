import React, { useState, useEffect } from 'react';
import { Share2, FileCode, Clipboard, Download, Check, Loader2 } from 'lucide-react';
import { M3Button } from '@shared/components/M3Button';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { M3SegmentedButton } from '@shared/components/M3SegmentedButton';
import { profileService } from '@infra/authorization/services/profile.service';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  profileId: string;
}

type ExportFormat = 'JSON' | 'XML' | 'CSV';

export const M3AuthorizationGraphPanel: React.FC<Props> = ({ isOpen, onClose, profileId }) => {
  const [format, setFormat] = useState<ExportFormat>('JSON');
  const [content, setContent] = useState('');
  const [loading, setLoading] = useState(false);
  const [copied, setCopied] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!profileId || !isOpen) return;

    const fetchGraph = async () => {
      setLoading(true);
      setError('');
      try {
        const rawFormat = format.toLowerCase() as 'json' | 'xml' | 'csv';
        const res = await profileService.exportGraph(profileId, rawFormat);
        
        // If JSON, format nicely just in case
        if (format === 'JSON') {
          try {
            const parsed = JSON.parse(res);
            setContent(JSON.stringify(parsed, null, 2));
          } catch {
            setContent(res);
          }
        } else {
          setContent(res);
        }
      } catch {
        setError('Error al generar el grafo en el servidor.');
      } finally {
        setLoading(false);
      }
    };

    fetchGraph();
  }, [profileId, format, isOpen]);

  const handleCopy = () => {
    if (!content) return;
    navigator.clipboard.writeText(content);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleDownload = () => {
    if (!content) return;
    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `profile_${profileId}.${format.toLowerCase()}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title="Grafo de Autorización Efectiva (AOP Factory)"
      icon={<Share2 className="w-4 h-4 text-m3-primary" />}
      maxWidth="max-w-4xl"
      footer={
        <>
          <M3Button type="button" variant="text" onClick={onClose}>
            Cerrar
          </M3Button>
          <M3Button
            type="button"
            variant="tonal"
            onClick={handleCopy}
            disabled={loading || !content}
            className="mr-2"
          >
            {copied ? (
              <>
                <Check className="w-3.5 h-3.5 mr-1.5 text-emerald-500" />
                Copiado
              </>
            ) : (
              <>
                <Clipboard className="w-3.5 h-3.5 mr-1.5" />
                Copiar
              </>
            )}
          </M3Button>
          <M3Button
            type="button"
            variant="filled"
            onClick={handleDownload}
            disabled={loading || !content}
          >
            <Download className="w-3.5 h-3.5 mr-1.5" />
            Descargar .{format.toLowerCase()}
          </M3Button>
        </>
      }
    >
      <div className="space-y-4">
        <p className="text-[11px] text-m3-secondary">
          Este grafo consolidado y jerárquico representa las asignaciones de acceso efectivas resueltas dinámicamente en el servidor backend mediante nuestro framework corporativo <span className="font-bold">Ums.Shell.Factory</span>.
        </p>

        {/* Format Selector using our standard segment bar */}
        <div className="flex justify-center">
          <M3SegmentedButton
            segments={[
              { id: 'JSON', label: 'JSON Graph' },
              { id: 'XML', label: 'XML Tree' },
              { id: 'CSV', label: 'CSV Tabular' },
            ]}
            activeId={format}
            onChange={(id) => setFormat(id as ExportFormat)}
          />
        </div>

        {/* Preview Screen */}
        <div className="relative rounded-xl border border-m3-outline/20 bg-zinc-950 p-4 font-mono text-xs text-zinc-300 min-h-[250px] max-h-[400px] overflow-auto shadow-inner">
          {loading ? (
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-2 bg-zinc-950/80">
              <Loader2 className="w-7 h-7 animate-spin text-m3-primary" />
              <span className="text-zinc-400 text-xs">Invocando AOP Factory...</span>
            </div>
          ) : error ? (
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-2 p-4 text-center text-rose-500 bg-zinc-950/80">
              <FileCode className="w-8 h-8 opacity-40" />
              <span>{error}</span>
            </div>
          ) : !content ? (
            <div className="absolute inset-0 flex flex-col items-center justify-center text-zinc-500 bg-zinc-950/80">
              No hay datos para visualizar.
            </div>
          ) : (
            <pre className="whitespace-pre overflow-x-auto text-[11px] leading-relaxed">
              {content}
            </pre>
          )}
        </div>
      </div>
    </M3FormDialog>
  );
};
