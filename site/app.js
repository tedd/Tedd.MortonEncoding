const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

if (reducedMotion || !("IntersectionObserver" in window)) {
  document.querySelectorAll(".reveal").forEach((element) => element.classList.add("visible"));
} else {
  const observer = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      entry.target.classList.add("visible");
      observer.unobserve(entry.target);
    }
  }, { threshold: 0.12 });
  document.querySelectorAll(".reveal").forEach((element) => observer.observe(element));
}

const modes = {
  "2d32": { dimensions: 2, bitsPerAxis: 16, codeBits: 32, method: "Encode(x, y)" },
  "3d32": { dimensions: 3, bitsPerAxis: 10, codeBits: 32, method: "Encode(x, y, z)" },
  "2d64": { dimensions: 2, bitsPerAxis: 32, codeBits: 64, method: "Encode64(x, y)" },
  "3d64": { dimensions: 3, bitsPerAxis: 21, codeBits: 64, method: "Encode64(x, y, z)" }
};

let selectedMode = "2d32";
const inputs = [
  document.querySelector("#coordinate-x"),
  document.querySelector("#coordinate-y"),
  document.querySelector("#coordinate-z")
];
const zField = document.querySelector(".z-input");
const coordinateInputs = document.querySelector(".coordinate-inputs");
const methodOutput = document.querySelector("#method-output");
const decimalOutput = document.querySelector("#decimal-output");
const hexOutput = document.querySelector("#hex-output");
const binaryOutput = document.querySelector("#binary-output");
const capacityOutput = document.querySelector("#capacity-output");
const svg = document.querySelector("#morton-curve");
const svgNamespace = "http://www.w3.org/2000/svg";

function interleave(coordinates, bitsPerAxis) {
  let result = 0n;
  const dimensions = coordinates.length;
  for (let bit = 0; bit < bitsPerAxis; bit += 1) {
    for (let axis = 0; axis < dimensions; axis += 1) {
      const sourceBit = (coordinates[axis] >> BigInt(bit)) & 1n;
      result |= sourceBit << BigInt((bit * dimensions) + axis);
    }
  }
  return result;
}

function parseCoordinate(input, maximum) {
  let value;
  try {
    value = BigInt(input.value.trim() || "0");
  } catch {
    value = 0n;
  }
  if (value < 0n) value = 0n;
  if (value > maximum) value = maximum;
  input.value = value.toString();
  return value;
}

function groupBits(value) {
  return value.match(/.{1,4}/g).join(" ");
}

function svgElement(name, attributes) {
  const element = document.createElementNS(svgNamespace, name);
  for (const [key, value] of Object.entries(attributes)) element.setAttribute(key, value);
  return element;
}

function curvePoint(index) {
  let x = 0;
  let y = 0;
  for (let bit = 0; bit < 3; bit += 1) {
    x |= ((index >> (bit * 2)) & 1) << bit;
    y |= ((index >> ((bit * 2) + 1)) & 1) << bit;
  }
  const margin = 42;
  const step = (520 - (margin * 2)) / 7;
  return [margin + (x * step), 520 - margin - (y * step)];
}

function drawCurve(highlightIndex) {
  svg.replaceChildren();
  const margin = 42;
  const step = (520 - (margin * 2)) / 7;

  for (let index = 0; index < 8; index += 1) {
    const offset = margin + (index * step);
    svg.append(
      svgElement("line", { x1: offset, y1: margin, x2: offset, y2: 520 - margin, class: "curve-grid" }),
      svgElement("line", { x1: margin, y1: offset, x2: 520 - margin, y2: offset, class: "curve-grid" })
    );
  }

  const points = Array.from({ length: 64 }, (_, index) => curvePoint(index));
  const pointsAttribute = points.map((point) => point.join(",")).join(" ");
  const progressAttribute = points.slice(0, highlightIndex + 1).map((point) => point.join(",")).join(" ");
  svg.appendChild(svgElement("polyline", { points: pointsAttribute, class: "curve-line" }));
  if (highlightIndex > 0) svg.appendChild(svgElement("polyline", { points: progressAttribute, class: "curve-progress" }));

  const [x, y] = points[highlightIndex];
  svg.append(
    svgElement("circle", { cx: x, cy: y, r: 17, class: "curve-halo" }),
    svgElement("circle", { cx: x, cy: y, r: 6, class: "curve-point" })
  );
}

function updateEncoder() {
  const mode = modes[selectedMode];
  const maximum = (1n << BigInt(mode.bitsPerAxis)) - 1n;
  inputs.forEach((input) => input.setAttribute("max", maximum.toString()));
  zField.hidden = mode.dimensions === 2;
  coordinateInputs.classList.toggle("two-dimensional", mode.dimensions === 2);

  const coordinates = inputs.slice(0, mode.dimensions).map((input) => parseCoordinate(input, maximum));
  const code = interleave(coordinates, mode.bitsPerAxis);
  const hexadecimalWidth = mode.codeBits / 4;
  const binary = code.toString(2).padStart(mode.codeBits, "0");

  methodOutput.textContent = mode.method;
  decimalOutput.textContent = code.toString();
  hexOutput.textContent = `0x${code.toString(16).toUpperCase().padStart(hexadecimalWidth, "0")}`;
  binaryOutput.textContent = groupBits(binary);
  capacityOutput.textContent = `Each coordinate uses its least significant ${mode.bitsPerAxis} bits; values above ${maximum.toLocaleString("en-US")} are masked by the library.`;

  const localX = Number(coordinates[0] & 7n);
  const localY = Number(coordinates[1] & 7n);
  const highlightIndex = Number(interleave([BigInt(localX), BigInt(localY)], 3));
  drawCurve(highlightIndex);
}

document.querySelectorAll("[data-mode]").forEach((button) => {
  button.setAttribute("aria-pressed", String(button.classList.contains("active")));
  button.addEventListener("click", () => {
    selectedMode = button.dataset.mode;
    document.querySelectorAll("[data-mode]").forEach((candidate) => {
      const active = candidate === button;
      candidate.classList.toggle("active", active);
      candidate.setAttribute("aria-pressed", String(active));
    });
    updateEncoder();
  });
});

inputs.forEach((input) => input.addEventListener("input", updateEncoder));

document.querySelectorAll(".copy-code").forEach((button) => {
  button.addEventListener("click", async () => {
    const target = document.getElementById(button.dataset.copyTarget);
    const previousLabel = button.textContent;
    try {
      await navigator.clipboard.writeText(target.textContent.trim());
      button.textContent = "Copied";
    } catch {
      button.textContent = "Select text";
    }
    window.setTimeout(() => { button.textContent = previousLabel; }, 1600);
  });
});

updateEncoder();
