# SimpleCircuit

SimpleCircuit is a script that allows you to quickly sketch electronic circuits. It tries to fill in as many gaps as you want.

The application runs in the **browser** using Blazor. You can just go to it **[here](https://svenboulanger.github.io/SimpleCircuit/)** and start playing with it!

## Quick start

The format is relatively simple. Components (like resistors, capacitors, etc.) all have pins that can be connected using wires. You can usually just give those wires a pretty general direction. The following example starts from a **ground** (`GND`) component, and goes **up** (`<u>`) to a **resistor** (`R`). Note that we don't tell how much "up" the wire travels.

```
GND <u> R
```

The program fills in the length for you, by minimizing the wire length of all wires in a circuit (the default minimum wire length is 10pt).
The result looks like this:

<a href="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit.svg#gh-light-mode-only">
  <img src="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit.svg#gh-light-mode-only">
</a>
<a href="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit.svg#gh-dark-mode-only">
  <img src="https://svenboulanger.github.io/SimpleCircuit/images/sample_circuit_dark.svg#gh-dark-mode-only">
</a>

The output is immediately shown in the browser, and the SVG file can be downloaded using the download button. More advanced circuits require more complex alignments. For example, an inverting opamp circuit might be scripted like this:

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

This circuit has a **terminal** (`T`) that we give a label "in". We then go on to the right to an anonymous resistor `R`. Then we continue to **point** (`X`) Xminus, which is the point at the negative input of opamp (`OA`) OA1, and so on.

The notation `OA1[p]` represents the pin of opamp OA1 called `p`.
