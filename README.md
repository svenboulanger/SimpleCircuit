# SimpleCircuit

SimpleCircuit is a script that allows you to quickly sketch electronic circuits. It tries to fill in as many gaps as you want.

The application runs in the **browser** using Blazor WASM. It is statically hosted **[here](https://svenboulanger.github.io/SimpleCircuit/)** so you can start playing with it! The page will store your scripts on your local memory such that you don't accidentally lose your work. You can drag and drop exported SVG files back into the webpage to continue working on the associated script.

## Quick start

Components (like resistors, capacitors, etc.) have pins that can be connected using wires. The wires that are connected will also determine their orientation. The following example starts from a **ground** (`GND`) component, and goes **up** (`<u>`) to a **resistor** (`R`). Note that we don't have to tell SimpleCircuit how much "up" the wire travels. By default it will take a _minimum_ of 10 units.

```
GND <u> R
```

The result looks like this:

<a href="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit.svg#gh-light-mode-only">
  <img src="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit.svg#gh-light-mode-only">
</a>
<a href="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit.svg#gh-dark-mode-only">
  <img src="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit_dark.svg#gh-dark-mode-only">
</a>

Components can be anonymous/unnamed (like `R` or `C`) or they can be named by adding more characters (like `R1` or `Cj`). Only named components can be reused later in the netlist.
More advanced circuits require more complex alignments. For example, an inverting opamp circuit might be scripted like this:

```
T("in") <r> R("1k") <r> Xminus <r> OA1 <r> Xout
Xminus <u 20 r> R("1k") <r d> Xout
OA1[p] <l d> GND
Xout <r> T("out")
```

<a href="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit_2.svg#gh-light-mode-only">
  <img src="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit_2.svg#gh-light-mode-only">
</a>
<a href="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit_2.svg#gh-dark-mode-only">
  <img src="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit_2_dark.svg#gh-dark-mode-only">
</a>

The point `Xminus` is a named component because we want to reuse it for a different line of code. The notation `R("1k")` represents an anonymous resistor with the label "1k". The notation `OA1[p]` means pin "p" of component "OA1".
