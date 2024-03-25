// init-collections.js

// Check if 'MfaRegex' collection exists
if (!db.getCollectionNames().includes('MfaRegex')) {
  // Create MfaRegex collection and insert an initial document
  db.createCollection("MfaRegex");
  db.MfaRegex.insert({
    Pattern: "example-pattern",
    Digit: 6,
    Email: "example@example.com",
    CreatedAt: new Date()
  });
}

// Check if 'MfaOtp' collection exists
if (!db.getCollectionNames().includes('MfaOtp')) {
  // Create MfaOtp collection and insert an initial document
  db.createCollection("MfaOtp");
  db.MfaOtp.insert({
    Id: "unique-id-123",
    Token: "token123",
    ReceivedAt: new Date(),
    ReceivedBy: "user@example.com",
    SentBy: "service@example.com",
    CreatedAt: new Date(),
    ExpiredAt: new Date(new Date().getTime() + (30 * 60 * 1000)) // Expires in 30 minutes
  });
}

