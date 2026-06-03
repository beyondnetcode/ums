import React, { useState, useEffect } from 'react';
import { Share2, FileCode, Clipboard, Download, Check, Loader2 } from 'lucide-react';
import { M3Button } from '@shared/components/M3Button';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { M3SegmentedButton } from '@shared/components/M3SegmentedButton';
import { getHttpErrorMessage, getSupportReferenceId } from '@app/errors/http-error';
import { profileService } from '@infra/authorization/services/profile.service';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  profileId: string;
}

type ExportFormat = 'JSON' | 'XML' | 'YAML' | 'CSV';

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
        const rawFormat = format.toLowerCase() as 'json' | 'xml' | 'yaml' | 'csv';
        const res = await profileService.exportGraph(profileId, rawFormat);

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
      } catch (err) {
        const errorMessage = getHttpErrorMessage(err, 'Error al generar el grafo en el servidor.');
        const supportReferenceId = getSupportReferenceId(err);
        setError(supportReferenceId ? `${errorMessage} Referencia: ${supportReferenceId}` : errorMessage);
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
    const mimeTypes: Record<string, string> = {
      json: 'application/json',
      xml: 'application/xml',
      yaml: 'application/x-yaml',
      csv: 'text/csv',
    };
    const mimeType = mimeTypes[format.toLowerCase()] || 'text/plain';
    const blob = new Blob([content], { type: `${mimeType};charset=utf-8` });
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
      title="Grafo de Autorización Efectiva"
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

        <div className="flex justify-center">
          <M3SegmentedButton
            options={[
              { value: 'JSON', label: 'JSON' },
              { value: 'XML', label: 'XML' },
              { value: 'YAML', label: 'YAML' },
              { value: 'CSV', label: 'CSV' },
            ]}
            value={format}
            onChange={(v) => setFormat(v as ExportFormat)}
          />
        </div>

        <div className="relative rounded-xl border border-m3-outline/20 bg-zinc-950 p-4 font-mono text-[11px] text-zinc-300 min-h-[250px] max-h-[400px] overflow-auto shadow-inner">
          {loading ? (
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-2 bg-zinc-950/80">
              <Loader2 className="w-7 h-7 animate-spin text-m3-primary" />
              <span className="text-zinc-400 text-[11px]">Invocando AOP Factory...</span>
            </div>
          ) : error ? (
            <div className="absolute inset-0 flex flex-col items-center justify-center gap-2 p-4 text-center text-rose-500 bg-zinc-950/80">
              <FileCode className="w-8 h-8 opacity-40" />
              <span className="text-[11px]">{error}</span>
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
