import { FormEvent, useState } from "react";
import { ShareAccessLevel, ShareLink } from "../models";
import { ApiError, useServices } from "../services";

interface SharingSectionProps {
  tripId: string;
}

const accessLabels: Record<ShareAccessLevel, string> = {
  [ShareAccessLevel.View]: "Samo pregled",
  [ShareAccessLevel.Edit]: "Pregled i izmjena"
};

export function SharingSection({ tripId }: SharingSectionProps) {
  const { sharingService } = useServices();
  const [accessLevel, setAccessLevel] = useState(ShareAccessLevel.View);
  const [expiresAtUtc, setExpiresAtUtc] = useState("");
  const [shareLink, setShareLink] = useState<ShareLink | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);

    if (!expiresAtUtc) {
      setError("Datum isteka je obavezan.");
      return;
    }

    setIsSubmitting(true);
    try {
      const link = await sharingService.createShareLink({
        tripId,
        accessLevel,
        expiresAtUtc: new Date(expiresAtUtc).toISOString()
      });
      setShareLink(link);
    } catch (caughtError) {
      setError(caughtError instanceof ApiError ? caughtError.message : "Link za dijeljenje nije kreiran.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="management-section">
      <div className="section-header">
        <h2>Dijeljenje plana</h2>
      </div>

      {error && <p className="form-error">{error}</p>}

      <form className="trip-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <label>
            Nivo pristupa
            <select
              value={accessLevel}
              onChange={(event) => setAccessLevel(Number(event.target.value) as ShareAccessLevel)}
            >
              {Object.values(ShareAccessLevel)
                .filter((level): level is ShareAccessLevel => typeof level === "number")
                .map((level) => (
                  <option key={level} value={level}>
                    {accessLabels[level]}
                  </option>
                ))}
            </select>
          </label>
          <label>
            Istice
            <input
              type="datetime-local"
              value={expiresAtUtc}
              onChange={(event) => setExpiresAtUtc(event.target.value)}
            />
          </label>
        </div>
        <div className="form-actions">
          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? "Kreiranje..." : "Kreiraj share link"}
          </button>
        </div>
      </form>

      {shareLink && (
        <article className="share-result">
          <div>
            <h3>{accessLabels[shareLink.accessLevel]}</h3>
            <p>Istice: {formatDateTime(shareLink.expiresAtUtc)}</p>
          </div>
          <div className="qr-preview">
            <img alt="QR kod za dijeljenje plana" src={shareLink.qrCodeUrl} />
          </div>
          <label>
            Link
            <input readOnly value={shareLink.shareUrl} />
          </label>
          <label>
            QR kod URL
            <input readOnly value={shareLink.qrCodeUrl} />
          </label>
        </article>
      )}
    </section>
  );
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("sr-Latn-BA", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}
