// Specify your database name here
var myDb = db.getSiblingDB('MfaDataStore');

// Check if 'MfaRegex' collection exists in 'myCustomDBName' database
if (!myDb.getCollectionNames().includes('MfaRegex')) {
  // Create MfaRegex collection and insert an initial document in 'myCustomDBName'
  myDb.createCollection("MfaRegex");
  myDb.MfaRegex.insert({
    Pattern: "example-pattern",
    Digit: 6,
    Email: "example@example.com",
    CreatedAt: new Date()
  });
}

// Check if 'MfaOtp' collection exists in 'myCustomDBName' database
if (!myDb.getCollectionNames().includes('MfaOtp')) {
  // Create MfaOtp collection and insert an initial document in 'myCustomDBName'
  myDb.createCollection("MfaOtp");
  myDb.MfaOtp.insert({
    Id: new ObjectId(),
    Token: "token123",
    ReceivedAt: new Date(),
    ReceivedBy: "user@example.com",
    SentBy: "service@example.com",
    CreatedAt: new Date(),
    ExpiredAt: new Date(new Date().getTime() + (30 * 60 * 1000)) // Expires in 30 minutes
  });
}

