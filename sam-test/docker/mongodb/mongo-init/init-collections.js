// init-collections.js

// Create MfaRegex collection and insert an initial document
db.createCollection("MfaRegex");
db.MfaRegex.insert({
  Pattern: "example-pattern",
  Digit: 6,
  Email: "example@example.com",
  CreatedAt: new Date()
});

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
