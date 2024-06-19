const PROXY_CONFIG = [
  {
    context: ["/log-in"],
    target: "https://localhost:32776",
    secure: false,
    changeOrigin: true,
    logLevel: "debug"
  }
]

module.exports = PROXY_CONFIG;
