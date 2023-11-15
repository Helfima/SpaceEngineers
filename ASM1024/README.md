# ASM1024

Assembler language for Space Engineer.

# Syntax

In the documentation d? is use for a list of "ITerminalBlock" and v? for a variable name.

## Device IO

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>device d? Filter</td>
    <td>Creation a device (d?) with a filter, see filter section. a device is a list of "ITerminalBlock"</td>
  </tr>
  <tr>
    <td>get v? d? Property AgregationMode</td>
    <td>Get device property to variable, see AgregationMode</td>
  </tr>
  <tr>
    <td>inventory v? d? int Property AgregationMode</td>
    <td>get from a device inventory, the content property to variable, see AgregationMode</td>
  </tr>
  <tr>
    <td>set d? Property a(v?|num)</td>
    <td>Set variable value to property on device</td>
  </tr>
  <tr>
    <td>action d? ActionName</td>
    <td>Execute action on device with name "ActionName"</td>
  </tr>
  <tr>
    <td>color d? r(v?|num) g(v?|num) b(v?|num) a(v?|num)</td>
    <td>Set rgba color, values are integer [0-255]</td>
  </tr>
  <tr>
    <td>colorHSV d? h(v?|num) s(v?|num) v(v?|num)</td>
    <td>Set hsv color, h value are degre [0-360], s and v [0-1]</td>
  </tr>
  <tr>
    <td>colorHEX d? String</td>
    <td>Set html color</td>
  </tr>
</table>

### Device Filter

Sample intruction for create a device

```device plates_fixes "MG:Patins Fixes"```

Filter characters:

* C = Search By Contains
* G = Search Group
* M = Search on Multi Grid

### AgregationMode

Calcul agregation:

* 0 = Average
* 1 = Sum
* 2 = Minimum
* 3 = Maximum

## Jump

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
<td>j int</td>
<td>Jump execution to line a</td>
  </tr>
  <tr>
<td>jal int</td>
<td>Jump execution to line a and store next line number in ra</td>
  </tr>
  <tr>
<td>jr int</td>
<td>Relative jump to line a</td>
  </tr>
</table>

## Branch

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
<td>bap a(v?|num) b(v?|num) c(v?|num) d(v?|num)</td>
<td>Branch to line d if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>bapal a(v?|num) b(v?|num) c(v?|num) d(v?|num)</td>
<td>Branch to line c if a != b and store next line number in ra</td>
  </tr>
  <tr>
<td>bapz a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if abs(a) <= float.epsilon * 8</td>
  </tr>
  <tr>
<td>bapzal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if abs(a) <= float.epsilon * 8</td>
  </tr>
  <tr>
<td>beq a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a == b</td>
  </tr>
  <tr>
<td>beqal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a == b and store next line number in ra</td>
  </tr>
  <tr>
<td>beqz a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a == 0</td>
  </tr>
  <tr>
<td>beqzal a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a == 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>bge a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a >= b</td>
  </tr>
  <tr>
<td>bgeal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a >= b and store next line number in ra</td>
  </tr>
  <tr>
<td>bgez a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a >= 0</td>
  </tr>
  <tr>
<td>bgezal a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a >= 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>bgt a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a > b</td>
  </tr>
  <tr>
<td>bgtal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a > b and store next line number in ra</td>
  </tr>
  <tr>
<td>bgtz a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a > 0</td>
  </tr>
  <tr>
<td>bgtzal a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a > 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>ble a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a <= b</td>
  </tr>
  <tr>
<td>bleal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a <= b and store next line number in ra</td>
  </tr>
  <tr>
<td>blez a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a <= 0</td>
  </tr>
  <tr>
<td>blezal a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a <= 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>blt a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a < b</td>
  </tr>
  <tr>
<td>bltal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a < b and store next line number in ra</td>
  </tr>
  <tr>
<td>bltz a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a < 0</td>
  </tr>
  <tr>
<td>bltzal a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a < 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>bna a(v?|num) b(v?|num) c(v?|num) d(v?|num)</td>
<td>Branch to line d if abs(a - b) > max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>bnaal a(v?|num) b(v?|num) c(v?|num) d(v?|num)</td>
<td>Branch to line d if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8) and store next line number in ra</td>
  </tr>
  <tr>
<td>bnaz a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if abs(a) > float.epsilon * 8</td>
  </tr>
  <tr>
<td>bnazal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if abs(a) > float.epsilon * 8</td>
  </tr>
  <tr>
<td>bne a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a != b</td>
  </tr>
  <tr>
<td>bneal a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Branch to line c if a != b and store next line number in ra</td>
  </tr>
  <tr>
<td>bnez a(v?|num) b(v?|num)    </td>
<td>branch to line b if a != 0</td>
  </tr>
  <tr>
<td>bnezal a(v?|num) b(v?|num)    </td>
<td>Branch to line b if a != 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>brap a(v?|num) b(v?|num) c(v?|num) d(v?|num)</td>
<td>Relative branch to line d if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>brapz a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Relative branch to line c if abs(a) <= float.epsilon * 8</td>
  </tr>
  <tr>
<td>breq a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Relative branch to line c if a == b</td>
  </tr>
  <tr>
<td>breqz a(v?|num) b(v?|num)    </td>
<td>Relative branch to line b if a == 0</td>
  </tr>
  <tr>
<td>brge a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Relative jump to line c if a >= b</td>
  </tr>
  <tr>
<td>brgez a(v?|num) b(v?|num)    </td>
<td>Relative branch to line b if a >= 0</td>
  </tr>
  <tr>
<td>brgt a(v?|num) b(v?|num) c(v?|num)  </td>
<td>relative jump to line c if a > b</td>
  </tr>
  <tr>
<td>brgtz a(v?|num) b(v?|num)    </td>
<td>Relative branch to line b if a > 0</td>
  </tr>
  <tr>
<td>brle a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Relative jump to line c if a <= b</td>
  </tr>
  <tr>
<td>brlez a(v?|num) b(v?|num)    </td>
<td>Relative branch to line b if a <= 0</td>
  </tr>
  <tr>
<td>brlt a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Relative jump to line c if a < b</td>
  </tr>
  <tr>
<td>brltz a(v?|num) b(v?|num)    </td>
<td>Relative branch to line b if a < 0</td>
  </tr>
  <tr>
<td>brna a(v?|num) b(v?|num) c(v?|num) d(v?|num)</td>
<td>Relative branch to line d if abs(a - b) > max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>brnaz a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Relative branch to line c if abs(a) > float.epsilon * 8</td>
  </tr>
  <tr>
<td>brne a(v?|num) b(v?|num) c(v?|num)  </td>
<td>Relative branch to line c if a != b</td>
  </tr>
  <tr>
<td>brnez a(v?|num) b(v?|num)    </td>
<td>Relative branch to line b if a != 0</td>
  </tr>
</table>

## Selection

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>sapz v? a(v?|num) b(v?|num) </td>
    <td>Variable = 1 if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8), otherwise 0</td>
  </tr>
  <tr>
    <td>sapz v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if |a| <= float.epsilon * 8, otherwise 0</td>
  </tr>
  <tr>
    <td>select v? a(v?|num) b(v?|num) c(v?|num)</td>
    <td>Variable = b if a is non-zero, otherwise c</td>
  </tr>
  <tr>
    <td>seq v? a(v?|num) b(v?|num) </td>
    <td>Variable = 1 if a == b, otherwise 0</td>
  </tr>
  <tr>
    <td>seqz v? a(v?|num) </td>
    <td>Variable = 1 if a == 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sge v? a(v?|num) b(v?|num) </td>
    <td>Variable = 1 if a >= b, otherwise 0</td>
  </tr>
  <tr>
    <td>sgez v? a(v?|num)</td>
    <td>Variable = 1 if a >= 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sgt v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if a > b, otherwise 0</td>
  </tr>
  <tr>
    <td>sgtz v? a(v?|num) </td>
    <td>Variable = 1 if a > 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sle v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if a <= b, otherwise 0</td>
  </tr>
  <tr>
    <td>slez v? a(v?|num)  </td>
    <td>Variable = 1 if a <= 0, otherwise 0</td>
  </tr>
  <tr>
    <td>slt v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if a < b, otherwise 0</td>
  </tr>
  <tr>
    <td>sltz v? a(v?|num)</td>
    <td>Variable = 1 if a < 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sna v? a(v?|num) b(v?|num) c(v?|num)</td>
    <td>Variable = 1 if abs(a - b) > max(c * max(abs(a), abs(b)), float.epsilon * 8), otherwise 0</td>
  </tr>
  <tr>
    <td>snaz v? a(v?|num) b(v?|num) </td>
    <td>Variable = 1 if |a| > float.epsilon, otherwise 0</td>
  </tr>
  <tr>
    <td>sne v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if a != b, otherwise 0</td>
  </tr>
  <tr>
    <td>snez v? a(v?|num)</td>
    <td>Variable = 1 if a != 0, otherwise 0</td>
  </tr>
</table>

## Mathematical

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>abs v? a(v?|num)</td>
    <td>Variable = the absolute value of a</td>
  </tr>
  <tr>
    <td>acos v? a(v?|num) </td>
    <td>Returns the cosine of the specified angle</td>
  </tr>
  <tr>
    <td>add v? a(v?|num) b(v?|num) </td>
    <td>Variable = a + b.</td>
  </tr>
  <tr>
    <td>asin v? a(v?|num) </td>
    <td>Returns the angle whos sine is the specified value</td>
  </tr>
  <tr>
    <td>atan v? a(v?|num)</td>
    <td>Returns the angle whos tan is the specified value</td>
  </tr>
  <tr>
    <td>ceil v? a(v?|num)</td>
    <td>Variable = smallest integer greater than a</td>
  </tr>
  <tr>
    <td>cos v? a(v?|num)</td>
    <td>Returns the cosine of the specified angle</td>
  </tr>
  <tr>
    <td>div v? a(v?|num) b(v?|num)</td>
    <td>Variable = a / b</td>
  </tr>
  <tr>
    <td>exp v? a(v?|num)</td>
    <td>Variable = exp(a)</td>
  </tr>
  <tr>
    <td>floor v? a(v?|num)</td>
    <td>Variable = largest integer less than a</td>
  </tr>
  <tr>
    <td>log v? a(v?|num)</td>
    <td>Variable = log(a)</td>
  </tr>
  <tr>
    <td>max v? a(v?|num) b(v?|num)</td>
    <td>Variable = max of a or b</td>
  </tr>
  <tr>
    <td>min v? a(v?|num) b(v?|num)</td>
    <td>Variable = min of a or b</td>
  </tr>
  <tr>
    <td>mod v? a(v?|num) b(v?|num)</td>
    <td>Variable = a mod b (note: NOT a % b)</td>
  </tr>
  <tr>
    <td>mul v? a(v?|num) b(v?|num)</td>
    <td>Variable = a * b</td>
  </tr>
  <tr>
    <td>rand v?</td>
    <td>Variable = a random value x with 0 <= x < 1</td>
  </tr>
  <tr>
    <td>round v? a(v?|num)</td>
    <td>Variable = a rounded to nearest integer</td>
  </tr>
  <tr>
    <td>sin v? a(v?|num)</td>
    <td>Returns the sine of the specified angle</td>
  </tr>
  <tr>
    <td>sqrt v? a(v?|num)</td>
    <td>Variable = square root of a</td>
  </tr>
  <tr>
    <td>sub v? a(v?|num) b(v?|num)</td>
    <td>Variable = a - b.</td>
  </tr>
  <tr>
    <td>tan v? a(v?|num) </td>
    <td>Returns the tan of the specified angle</td>
  </tr>
  <tr>
    <td>trunc v? a(v?|num)</td>
    <td>Variable = a with fractional part removed</td>
  </tr>
</table>

## Logic

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>and v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if a and b not zero, otherwise 0</td>
  </tr>
  <tr>
    <td>nor v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if a and b are 0, otherwise 0</td>
  </tr>
  <tr>
    <td>or v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if a and/or b not 0, otherwise 0</td>
  </tr>
  <tr>
    <td>xor v? a(v?|num) b(v?|num)</td>
    <td>Variable = 1 if either a or b not 0, otherwise 0</td>
  </tr>
</table>

## Misc

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>define str num</td>    
    <td>Creates a label that will be replaced throughout the program with the provided value.</td>    
  </tr>
  <tr>
    <td>move v? a(v?|num)</td>    
    <td>Variable = provided num or Variable value.</td>    
  </tr>
  <tr>
    <td>yield</td>    
    <td>Pauses execution for 1 tick</td>    
  </tr>
  <tr>
    <td>print v?</td>    
    <td>Print Variable in the console</td>    
  </tr>
</table>
