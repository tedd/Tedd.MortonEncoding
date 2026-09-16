(() => {
  "use strict";

  const canvas = document.querySelector("#locality-canvas");
  const stage = canvas?.closest(".voxel-stage");
  const runButton = document.querySelector("#run-locality-demo");
  const resetButton = document.querySelector("#reset-locality-view");
  const coordinateOutput = document.querySelector("#demo-coordinate");
  const statusOutput = document.querySelector("#demo-status");
  const mortonOutput = document.querySelector("#morton-address");
  const rowOutput = document.querySelector("#row-address");
  const mortonMemory = document.querySelector("#morton-memory");
  const rowMemory = document.querySelector("#row-memory");

  if (!canvas || !stage || !runButton || !resetButton || !coordinateOutput || !statusOutput || !mortonMemory || !rowMemory) return;

  const context = canvas.getContext("2d");
  if (!context) return;

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const defaultView = { yaw: -0.72, pitch: -0.52 };
  const view = { ...defaultView };
  const points = [];
  const pointByKey = new Map();
  const memoryCells = { morton: new Map(), row: new Map() };
  const targetCoordinates = Array.from({ length: 8 }, (_, morton) => {
    const x = morton & 1;
    const y = (morton >> 1) & 1;
    const z = (morton >> 2) & 1;
    return { x, y, z, morton, row: x + (4 * y) + (16 * z) };
  });
  const targetKeys = new Set(targetCoordinates.map(({ x, y, z }) => `${x}:${y}:${z}`));
  let activeStep = -1;
  let selectedCoordinate = null;
  let animationTimer = 0;
  let dragging = false;
  let previousPointer = { x: 0, y: 0 };

  for (let z = 0; z < 4; z += 1) {
    for (let y = 0; y < 4; y += 1) {
      for (let x = 0; x < 4; x += 1) {
        const point = { x, y, z, key: `${x}:${y}:${z}` };
        points.push(point);
        pointByKey.set(point.key, point);
      }
    }
  }

  function mortonAddress(x, y, z) {
    return (x & 1)
      | ((y & 1) << 1)
      | ((z & 1) << 2)
      | ((x & 2) << 2)
      | ((y & 2) << 3)
      | ((z & 2) << 4);
  }

  function coordinateFromAddress(layout, address) {
    if (layout === "row") {
      const x = address & 3;
      const y = (address >> 2) & 3;
      const z = (address >> 4) & 3;
      return { x, y, z, morton: mortonAddress(x, y, z), row: address };
    }

    const x = (address & 1) | (((address >> 3) & 1) << 1);
    const y = ((address >> 1) & 1) | (((address >> 4) & 1) << 1);
    const z = ((address >> 2) & 1) | (((address >> 5) & 1) << 1);
    return { x, y, z, morton: address, row: x + (4 * y) + (16 * z) };
  }

  function createMemoryMap(container, layout) {
    for (let line = 0; line < 4; line += 1) {
      const lineElement = document.createElement("div");
      lineElement.className = "cache-line";
      lineElement.dataset.line = String(line);

      const label = document.createElement("span");
      label.className = "cache-line-label";
      label.textContent = `line ${line}`;
      lineElement.appendChild(label);

      for (let offset = 0; offset < 8; offset += 1) {
        const address = (line * 8) + offset;
        const cell = document.createElement("button");
        cell.type = "button";
        cell.className = "memory-cell";
        cell.textContent = String(address);
        cell.dataset.address = String(address);
        cell.setAttribute("aria-label", `${layout === "morton" ? "Morton" : "Row-major"} address ${address}`);
        cell.setAttribute("aria-pressed", "false");
        cell.addEventListener("click", () => selectAddress(layout, address));
        lineElement.appendChild(cell);
        memoryCells[layout].set(address, cell);
      }

      container.appendChild(lineElement);
    }
  }

  createMemoryMap(mortonMemory, "morton");
  createMemoryMap(rowMemory, "row");

  function updateMemory() {
    const visited = activeStep < 0 ? [] : targetCoordinates.slice(0, activeStep + 1);
    const current = activeStep < 0 ? null : targetCoordinates[activeStep];
    const selected = selectedCoordinate;

    for (const layout of ["morton", "row"]) {
      const targetAddresses = new Set(targetCoordinates.map((coordinate) => coordinate[layout]));
      const visitedAddresses = new Set(visited.map((coordinate) => coordinate[layout]));

      for (const [address, cell] of memoryCells[layout]) {
        cell.classList.toggle("target", targetAddresses.has(address));
        cell.classList.toggle("visited", visitedAddresses.has(address));
        cell.classList.toggle("current", current?.[layout] === address);
        cell.classList.toggle("selected", selected?.[layout] === address);
        cell.setAttribute("aria-pressed", String(selected?.[layout] === address));
      }

      const activeLines = new Set([
        ...visited.map((coordinate) => Math.floor(coordinate[layout] / 8)),
        ...(selected ? [Math.floor(selected[layout] / 8)] : [])
      ]);
      const container = layout === "morton" ? mortonMemory : rowMemory;
      container.querySelectorAll(".cache-line").forEach((lineElement) => {
        lineElement.classList.toggle("line-active", activeLines.has(Number(lineElement.dataset.line)));
      });
    }

    if (selected && !current) {
      const source = selected.source === "morton" ? "Morton-order" : "x-major row";
      coordinateOutput.textContent = `Selected coordinate (${selected.x}, ${selected.y}, ${selected.z})`;
      statusOutput.textContent = `Selected from ${source} address ${selected[selected.source]}. The equivalent addresses are Morton ${selected.morton} and row-major ${selected.row}.`;
      mortonOutput.textContent = `address ${selected.morton}`;
      rowOutput.textContent = `address ${selected.row}`;
      return;
    }

    if (!current) {
      coordinateOutput.textContent = "Ready: eight spatial neighbors";
      statusOutput.textContent = "Select any memory address to locate its coordinate; select “Run demo” to compare the eight neighbors.";
      mortonOutput.textContent = "0–7";
      rowOutput.textContent = "0, 1, 4, 5, 16, 17, 20, 21";
      return;
    }

    coordinateOutput.textContent = `Coordinate (${current.x}, ${current.y}, ${current.z})`;
    statusOutput.textContent = `Step ${activeStep + 1} of 8: Morton address ${current.morton}; row-major address ${current.row}.`;
    mortonOutput.textContent = `address ${current.morton}`;
    rowOutput.textContent = `address ${current.row}`;

    if (activeStep === targetCoordinates.length - 1) {
      coordinateOutput.textContent = "Eight neighboring coordinates visited";
      statusOutput.textContent = "Morton order used one cache line; x-major row order used two cache lines in this model.";
      mortonOutput.textContent = "contiguous 0–7";
      rowOutput.textContent = "two separated groups";
    }
  }

  function rotatePoint(point) {
    const centeredX = point.x - 1.5;
    const centeredY = point.y - 1.5;
    const centeredZ = point.z - 1.5;
    const cosYaw = Math.cos(view.yaw);
    const sinYaw = Math.sin(view.yaw);
    const cosPitch = Math.cos(view.pitch);
    const sinPitch = Math.sin(view.pitch);
    const rotatedX = (centeredX * cosYaw) - (centeredZ * sinYaw);
    const yawDepth = (centeredX * sinYaw) + (centeredZ * cosYaw);
    const rotatedY = (centeredY * cosPitch) - (yawDepth * sinPitch);
    const depth = (centeredY * sinPitch) + (yawDepth * cosPitch);
    return { x: rotatedX, y: rotatedY, depth };
  }

  function projectionParameters() {
    const width = canvas.clientWidth;
    const height = canvas.clientHeight;
    return {
      width,
      height,
      centerX: width / 2,
      centerY: height * 0.51,
      scale: Math.min(width, height) * 0.155
    };
  }

  function project(point, parameters) {
    const rotated = rotatePoint(point);
    const perspective = 7.5 / (7.5 + rotated.depth);
    return {
      x: parameters.centerX + (rotated.x * parameters.scale * perspective),
      y: parameters.centerY - (rotated.y * parameters.scale * perspective),
      depth: rotated.depth,
      perspective
    };
  }

  function drawLine(from, to, color, width, parameters) {
    const start = project(from, parameters);
    const end = project(to, parameters);
    context.beginPath();
    context.moveTo(start.x, start.y);
    context.lineTo(end.x, end.y);
    context.strokeStyle = color;
    context.lineWidth = width;
    context.stroke();
  }

  function drawGrid(parameters) {
    const axisColors = {
      x: "rgba(101, 210, 198, .19)",
      y: "rgba(230, 165, 124, .17)",
      z: "rgba(147, 197, 244, .17)"
    };

    for (const point of points) {
      if (point.x < 3) drawLine(point, pointByKey.get(`${point.x + 1}:${point.y}:${point.z}`), axisColors.x, 1, parameters);
      if (point.y < 3) drawLine(point, pointByKey.get(`${point.x}:${point.y + 1}:${point.z}`), axisColors.y, 1, parameters);
      if (point.z < 3) drawLine(point, pointByKey.get(`${point.x}:${point.y}:${point.z + 1}`), axisColors.z, 1, parameters);
    }
  }

  function drawOctant(parameters) {
    const corners = [];
    for (const z of [-0.25, 1.25]) {
      for (const y of [-0.25, 1.25]) {
        for (const x of [-0.25, 1.25]) corners.push({ x, y, z });
      }
    }

    const edges = [
      [0, 1], [0, 2], [0, 4], [1, 3], [1, 5], [2, 3], [2, 6],
      [3, 7], [4, 5], [4, 6], [5, 7], [6, 7]
    ];
    for (const [from, to] of edges) drawLine(corners[from], corners[to], "rgba(101, 210, 198, .72)", 1.5, parameters);
  }

  function drawPath(parameters) {
    const visibleCount = activeStep < 0 ? targetCoordinates.length : activeStep + 1;
    context.lineCap = "round";
    context.lineJoin = "round";

    for (let index = 1; index < visibleCount; index += 1) {
      const previous = targetCoordinates[index - 1];
      const current = targetCoordinates[index];
      drawLine(previous, current, activeStep < 0 ? "rgba(101, 210, 198, .18)" : "rgba(101, 210, 198, .9)", activeStep < 0 ? 2 : 3, parameters);
    }
  }

  function drawPoints(parameters) {
    const projectedPoints = points.map((point) => ({
      ...point,
      projected: project(point, parameters)
    })).sort((left, right) => right.projected.depth - left.projected.depth);
    const current = activeStep < 0 ? null : targetCoordinates[activeStep];
    const selected = selectedCoordinate;

    for (const point of projectedPoints) {
      const target = targetKeys.has(point.key);
      const targetIndex = target ? targetCoordinates.findIndex((coordinate) => `${coordinate.x}:${coordinate.y}:${coordinate.z}` === point.key) : -1;
      const visited = targetIndex >= 0 && activeStep >= targetIndex;
      const isCurrent = current && current.x === point.x && current.y === point.y && current.z === point.z;
      const isSelected = selected && selected.x === point.x && selected.y === point.y && selected.z === point.z;
      const radius = (target || isSelected ? 6.3 : 3.2) * point.projected.perspective;

      if (isCurrent || isSelected) {
        context.beginPath();
        context.arc(point.projected.x, point.projected.y, radius + 9, 0, Math.PI * 2);
        context.fillStyle = isSelected ? "rgba(147, 197, 244, .18)" : "rgba(142, 225, 215, .16)";
        context.fill();
      }

      context.beginPath();
      context.arc(point.projected.x, point.projected.y, radius, 0, Math.PI * 2);
      context.fillStyle = isSelected ? "#dceeff" : isCurrent ? "#e5fffb" : visited ? "#65d2c6" : target ? "#277f79" : "#547076";
      context.fill();
      context.strokeStyle = isSelected ? "#93c5f4" : isCurrent ? "#65d2c6" : target ? "#8ee1d7" : "rgba(181, 211, 208, .35)";
      context.lineWidth = isCurrent || isSelected ? 3 : 1;
      context.stroke();
    }

    if (current) drawCoordinateLabel(current, parameters, "#65d2c6");
    else if (selected) drawCoordinateLabel(selected, parameters, "#93c5f4");
  }

  function drawCoordinateLabel(coordinate, parameters, accent) {
    const projected = project(coordinate, parameters);
    const label = `(${coordinate.x}, ${coordinate.y}, ${coordinate.z})`;
    context.font = "600 12px 'DM Mono', monospace";
    const width = context.measureText(label).width + 20;
    const x = Math.min(parameters.width - width - 10, projected.x + 14);
    const y = Math.max(12, projected.y - 38);
    context.fillStyle = "rgba(11, 29, 34, .94)";
    context.strokeStyle = accent;
    context.lineWidth = 1;
    context.beginPath();
    context.roundRect(x, y, width, 30, 3);
    context.fill();
    context.stroke();
    context.fillStyle = "#dffaf6";
    context.fillText(label, x + 10, y + 19);
  }

  function render() {
    const parameters = projectionParameters();
    context.clearRect(0, 0, parameters.width, parameters.height);
    drawGrid(parameters);
    drawOctant(parameters);
    drawPath(parameters);
    drawPoints(parameters);
  }

  function resizeCanvas() {
    const ratio = Math.min(window.devicePixelRatio || 1, 2);
    const width = Math.max(1, Math.round(stage.clientWidth));
    const height = Math.max(1, Math.round(stage.clientHeight));
    const pixelWidth = Math.round(width * ratio);
    const pixelHeight = Math.round(height * ratio);
    if (canvas.width !== pixelWidth || canvas.height !== pixelHeight) {
      canvas.width = pixelWidth;
      canvas.height = pixelHeight;
    }
    context.setTransform(ratio, 0, 0, ratio, 0, 0);
    render();
  }

  function finishDemo() {
    activeStep = targetCoordinates.length - 1;
    runButton.firstChild.textContent = "Replay demo ";
    updateMemory();
    render();
  }

  function advanceDemo() {
    activeStep += 1;
    updateMemory();
    render();

    if (activeStep < targetCoordinates.length - 1) {
      animationTimer = window.setTimeout(advanceDemo, 620);
    } else {
      finishDemo();
    }
  }

  function runDemo() {
    window.clearTimeout(animationTimer);
    activeStep = -1;
    selectedCoordinate = null;
    runButton.firstChild.textContent = "Running ";
    if (reducedMotion) {
      finishDemo();
      return;
    }
    advanceDemo();
  }

  function resetView() {
    view.yaw = defaultView.yaw;
    view.pitch = defaultView.pitch;
    render();
    canvas.focus({ preventScroll: true });
  }

  function selectAddress(layout, address) {
    window.clearTimeout(animationTimer);
    activeStep = -1;
    const coordinate = coordinateFromAddress(layout, address);
    const selectingCurrentCoordinate = selectedCoordinate
      && selectedCoordinate.x === coordinate.x
      && selectedCoordinate.y === coordinate.y
      && selectedCoordinate.z === coordinate.z;
    selectedCoordinate = selectingCurrentCoordinate ? null : { ...coordinate, source: layout };
    runButton.firstChild.textContent = "Run demo ";
    updateMemory();
    render();
  }

  canvas.addEventListener("pointerdown", (event) => {
    dragging = true;
    previousPointer = { x: event.clientX, y: event.clientY };
    canvas.setPointerCapture(event.pointerId);
  });

  canvas.addEventListener("pointermove", (event) => {
    if (!dragging) return;
    view.yaw += (event.clientX - previousPointer.x) * 0.009;
    view.pitch = Math.max(-1.28, Math.min(1.28, view.pitch + ((event.clientY - previousPointer.y) * 0.009)));
    previousPointer = { x: event.clientX, y: event.clientY };
    render();
  });

  const stopDragging = (event) => {
    if (!dragging) return;
    dragging = false;
    if (canvas.hasPointerCapture(event.pointerId)) canvas.releasePointerCapture(event.pointerId);
  };
  canvas.addEventListener("pointerup", stopDragging);
  canvas.addEventListener("pointercancel", stopDragging);

  canvas.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && selectedCoordinate) {
      selectedCoordinate = null;
      updateMemory();
      render();
      return;
    }
    const change = event.shiftKey ? 0.18 : 0.09;
    if (event.key === "ArrowLeft") view.yaw -= change;
    else if (event.key === "ArrowRight") view.yaw += change;
    else if (event.key === "ArrowUp") view.pitch = Math.max(-1.28, view.pitch - change);
    else if (event.key === "ArrowDown") view.pitch = Math.min(1.28, view.pitch + change);
    else return;
    event.preventDefault();
    render();
  });

  runButton.addEventListener("click", runDemo);
  resetButton.addEventListener("click", resetView);
  if ("ResizeObserver" in window) {
    new ResizeObserver(resizeCanvas).observe(stage);
  } else {
    window.addEventListener("resize", resizeCanvas);
  }
  updateMemory();
  resizeCanvas();
})();
