import { describe, it, expect, beforeEach, afterEach } from "vitest";
import * as utils from "./utils";

describe("getIvyHost", () => {
  const defaultOrigin = window.location.origin;

  const setIvyHostMeta = (value: string) => {
    const meta = document.createElement("meta");
    meta.setAttribute("name", "ivy-host");
    meta.setAttribute("content", value);
    document.head.appendChild(meta);
  };

  beforeEach(() => {
    document.head.innerHTML = "";
  });

  afterEach(() => {
    document.head.innerHTML = "";
  });

  it("returns the meta host when it matches the current hostname", () => {
    const originalOrigin = window.location.origin;
    Object.defineProperty(window, "location", {
      value: {
        ...window.location,
        origin: "https://localhost:5173",
        hostname: "localhost",
      },
      writable: true,
      configurable: true,
    });

    try {
      setIvyHostMeta("https://localhost:5173");
      expect(utils.getIvyHost()).toBe("https://localhost:5173");
    } finally {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: originalOrigin,
        },
        writable: true,
        configurable: true,
      });
    }
  });

  it("supports hostname-only meta values by assuming https", () => {
    const originalOrigin = window.location.origin;
    Object.defineProperty(window, "location", {
      value: {
        ...window.location,
        origin: "https://localhost",
        hostname: "localhost",
      },
      writable: true,
      configurable: true,
    });

    try {
      setIvyHostMeta("localhost");
      expect(utils.getIvyHost()).toBe("https://localhost");
    } finally {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: originalOrigin,
        },
        writable: true,
        configurable: true,
      });
    }
  });

  it("falls back to the current origin when the meta host is not allowed", () => {
    setIvyHostMeta("https://malicious.example.com");
    expect(utils.getIvyHost()).toBe(defaultOrigin);
  });

  it("falls back when the meta tag contains an invalid value", () => {
    setIvyHostMeta("not a url");
    expect(utils.getIvyHost()).toBe(defaultOrigin);
  });

  it("falls back when no meta tag is present", () => {
    expect(utils.getIvyHost()).toBe(defaultOrigin);
  });

  describe("localhost variant matching for development", () => {
    const originalOrigin = window.location.origin;
    const originalHostname = window.location.hostname;

    beforeEach(() => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    afterEach(() => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    it("allows localhost with different ports when current origin is localhost", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("http://localhost:8080");
    });

    it("rejects localhost with different protocols (security: prevent protocol downgrade)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://localhost:5173");
      expect(utils.getIvyHost()).toBe("https://localhost:5173");
    });

    it("rejects HTTPS when current origin is HTTP (security: prevent protocol upgrade)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://localhost:5173");
      expect(utils.getIvyHost()).toBe("http://localhost:5173");
    });

    it("allows localhost with same protocol but different ports", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://localhost:8080");
      expect(utils.getIvyHost()).toBe("https://localhost:8080");
    });

    it("allows 127.0.0.1 with different ports when current origin is 127.0.0.1", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://127.0.0.1:5173",
          hostname: "127.0.0.1",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://127.0.0.1:8080");
      expect(utils.getIvyHost()).toBe("http://127.0.0.1:8080");
    });

    it("allows localhost and 127.0.0.1 to match when protocols match (same logical host)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://127.0.0.1:8080");
      expect(utils.getIvyHost()).toBe("http://127.0.0.1:8080");
    });

    it("rejects localhost and 127.0.0.1 match when protocols differ (security)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://127.0.0.1:8080");
      expect(utils.getIvyHost()).toBe("https://localhost:5173");
    });

    it("allows 127.0.0.1 and localhost to match (reverse direction)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://127.0.0.1:5173",
          hostname: "127.0.0.1",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("http://localhost:8080");
    });

    it("allows [::1] (IPv6 localhost) with different ports", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://[::1]:5173",
          hostname: "[::1]",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://[::1]:8080");
      expect(utils.getIvyHost()).toBe("http://[::1]:8080");
    });

    it("allows localhost and [::1] to match when protocols match (same logical host)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://[::1]:8080");
      expect(utils.getIvyHost()).toBe("http://[::1]:8080");
    });

    it("rejects localhost and [::1] match when protocols differ (security)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://[::1]:8080");
      expect(utils.getIvyHost()).toBe("https://localhost:5173");
    });

    it("rejects non-localhost hosts even with same hostname pattern", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://example.com:8080");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects localhost when current origin is not localhost (security)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects non-localhost when current origin is localhost (security)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://example.com:8080");
      expect(utils.getIvyHost()).toBe("http://localhost:5173");
    });

    it("handles case-insensitive localhost matching", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://LOCALHOST:5173",
          hostname: "LOCALHOST",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("http://localhost:8080");
    });

    it("allows exact origin match to take precedence over localhost variant check", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://localhost:5173",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://localhost:5173");
      expect(utils.getIvyHost()).toBe("http://localhost:5173");
    });
  });

  describe("production environment security", () => {
    const originalOrigin = window.location.origin;
    const originalHostname = window.location.hostname;

    beforeEach(() => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    afterEach(() => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: originalOrigin,
          hostname: originalHostname,
        },
        writable: true,
        configurable: true,
      });
    });

    it("requires exact origin match in production (non-localhost)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://example.com");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects different ports in production (non-localhost)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://example.com:8080");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects different protocols in production (non-localhost)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://example.com");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects localhost variants in production environment", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects 127.0.0.1 in production environment", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://127.0.0.1:8080");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects IPv6 localhost in production environment", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("http://[::1]:8080");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects different subdomains in production", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://subdomain.example.com");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("rejects completely different domains in production", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://attacker.com");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("allows exact match with port in production when current origin has port", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com:8443",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://example.com:8443");
      expect(utils.getIvyHost()).toBe("https://example.com:8443");
    });

    it("rejects different port even when both have explicit ports in production", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com:8443",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://example.com:9443");
      expect(utils.getIvyHost()).toBe("https://example.com:8443");
    });

    it("handles production environment with custom domain", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://app.mycompany.com",
          hostname: "app.mycompany.com",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://app.mycompany.com");
      expect(utils.getIvyHost()).toBe("https://app.mycompany.com");

      setIvyHostMeta("https://app.mycompany.com:8080");
      expect(utils.getIvyHost()).toBe("https://app.mycompany.com");
    });

    it("handles production environment with IP address (non-localhost)", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://192.168.1.100",
          hostname: "192.168.1.100",
        },
        writable: true,
        configurable: true,
      });

      setIvyHostMeta("https://192.168.1.100");
      expect(utils.getIvyHost()).toBe("https://192.168.1.100");

      setIvyHostMeta("https://192.168.1.100:8080");
      expect(utils.getIvyHost()).toBe("https://192.168.1.100");
    });

    it("verifies localhost matching only works when current origin is localhost", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "http://localhost:3000",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("http://localhost:8080");

      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });

    it("verifies protocol matching requirement in both environments", () => {
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://localhost:3000",
          hostname: "localhost",
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta("http://localhost:8080");
      expect(utils.getIvyHost()).toBe("https://localhost:3000");

      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });
      setIvyHostMeta("http://example.com");
      expect(utils.getIvyHost()).toBe("https://example.com");
    });
  });

  describe("with ivy-path-base meta tag (base path behind reverse proxy)", () => {
    const setIvyBasePathMeta = (value: string) => {
      const meta = document.createElement("meta");
      meta.setAttribute("name", "ivy-path-base");
      meta.setAttribute("content", value);
      document.head.appendChild(meta);
    };

    it("appends base path with leading slash to origin", () => {
      setIvyBasePathMeta("/studio");
      expect(utils.getIvyHost()).toBe(defaultOrigin + "/studio");
    });

    it("appends base path to meta host origin", () => {
      const originalOrigin = window.location.origin;
      Object.defineProperty(window, "location", {
        value: {
          ...window.location,
          origin: "https://example.com",
          hostname: "example.com",
        },
        writable: true,
        configurable: true,
      });
      try {
        setIvyHostMeta("https://example.com");
        setIvyBasePathMeta("/ivy");
        expect(utils.getIvyHost()).toBe("https://example.com/ivy");
      } finally {
        Object.defineProperty(window, "location", {
          value: {
            ...window.location,
            origin: originalOrigin,
          },
          writable: true,
          configurable: true,
        });
      }
    });

    it("returns origin without base path when no base path meta", () => {
      expect(utils.getIvyHost()).toBe(defaultOrigin);
    });
  });
});

describe("getShellParam", () => {
  const originalLocation = window.location;

  function setSearch(search: string) {
    Object.defineProperty(window, "location", {
      value: { ...originalLocation, search },
      writable: true,
      configurable: true,
    });
  }

  afterEach(() => {
    Object.defineProperty(window, "location", {
      value: originalLocation,
      writable: true,
      configurable: true,
    });
  });

  it("should return true when no parameter is present", () => {
    setSearch("");
    expect(utils.getShellParam()).toBe(true);
  });

  it("should return false when shell=false", () => {
    setSearch("?shell=false");
    expect(utils.getShellParam()).toBe(false);
  });

  it("should return true when shell=true", () => {
    setSearch("?shell=true");
    expect(utils.getShellParam()).toBe(true);
  });

  it("should return false when chrome=false (backwards compatibility)", () => {
    setSearch("?chrome=false");
    expect(utils.getShellParam()).toBe(false);
  });

  it("should return true when chrome=true", () => {
    setSearch("?chrome=true");
    expect(utils.getShellParam()).toBe(true);
  });

  it("should prefer shell over chrome when both present", () => {
    setSearch("?shell=true&chrome=false");
    expect(utils.getShellParam()).toBe(true);
  });

  it("should prefer shell=false over chrome=true when both present", () => {
    setSearch("?shell=false&chrome=true");
    expect(utils.getShellParam()).toBe(false);
  });

  it("should be case-insensitive for shell value", () => {
    setSearch("?shell=FALSE");
    expect(utils.getShellParam()).toBe(false);
  });

  it("should be case-insensitive for chrome value", () => {
    setSearch("?chrome=False");
    expect(utils.getShellParam()).toBe(false);
  });
});
