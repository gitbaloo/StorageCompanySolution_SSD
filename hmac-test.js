const crypto = require("crypto");

const method = "POST";
const pathAndQuery = "/api/hmac-test";

const body = JSON.stringify({
    hello: "world"
});

const secret = "Placeholder-request-signing-secret-at-least-32-chars";

const timestamp = Math.floor(Date.now() / 1000).toString();
const nonce = crypto.randomUUID();

const bodyHash = crypto
    .createHash("sha256")
    .update(body, "utf8")
    .digest("hex");

const canonicalRequest =
    `${method}\n` +
    `${pathAndQuery}\n` +
    `${timestamp}\n` +
    `${nonce}\n` +
    `${bodyHash}`;

const signature = crypto
    .createHmac("sha256", secret)
    .update(canonicalRequest, "utf8")
    .digest("base64");

console.log("Paste this body into Scalar:");
console.log(body);
console.log();
console.log("Paste these headers into Scalar:");
console.log("X-Request-Timestamp:", timestamp);
console.log("X-Request-Nonce:", nonce);
console.log("X-Request-Signature:", signature);