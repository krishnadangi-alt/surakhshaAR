export default function Certificates() {
  return (
    <div className="placeholder-page">
      <div className="icon">{'\u{1F393}'}</div>
      <h2>Certificate Management</h2>
      <p className="text-sm text-muted" style={{ maxWidth: 400 }}>
        Certificate issuance and verification will be managed here.
        <br />
        Connect to the backend <code>/api/v1/certificates</code> endpoint when ready.
      </p>
    </div>
  );
}
