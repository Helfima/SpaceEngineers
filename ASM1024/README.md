# ASM1024

Assembler language for Space Engineer.

# Syntax

## Device IO

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>device r? Filter</td>
    <td>Creation a device (d?) with a filter, see filter section. a device is a list of "ITerminalBlock"</td>
  </tr>
  <tr>
    <td>load r? d? Property AgregationMode</td>
    <td>Load device property to register, AgregationMode:Average (0), Sum (1), Minimum (2), Maximum (3)</td>
  </tr>
  <tr>
    <td>inventory r? d? int Property AgregationMode</td>
    <td>Load from a device inventory, the content property to register, AgregationMode:Average (0), Sum (1), Minimum (2), Maximum (3)</td>
  </tr>
  <tr>
    <td>store d? Property a(r?|num)</td>
    <td></td>
  </tr>
  <tr>
    <td>action r? ActionName</td>
    <td>Execute action on device with name "ActionName"</td>
  </tr>
  <tr>
    <td>color r(r?|num)	g(r?|num)	b(r?|num)	a(r?|num)</td>
    <td>Store rgba color, values are integer [0-255]</td>
  </tr>
  <tr>
    <td>colorrainbow r? a(r?|num)</td>
    <td>Store color from interpolation value a</td>
  </tr>
</table>

### Device Filter

Sample intruction for create a device

```device plates_fixes "MG:Patins Fixes"```

Filter characters:

* C=Search By Contains
* G=Search Group
* M=Search on Multi Grid

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
<td>bap	a(r?|num)	b(r?|num)	c(r?|num)	d(r?|num)</td>
<td>Branch to line d if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>bapal	a(r?|num)	b(r?|num)	c(r?|num)	d(r?|num)</td>
<td>Branch to line c if a != b and store next line number in ra</td>
  </tr>
  <tr>
<td>bapz	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if abs(a) <= float.epsilon * 8</td>
  </tr>
  <tr>
<td>bapzal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if abs(a) <= float.epsilon * 8</td>
  </tr>
  <tr>
<td>beq	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a == b</td>
  </tr>
  <tr>
<td>beqal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a == b and store next line number in ra</td>
  </tr>
  <tr>
<td>beqz	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a == 0</td>
  </tr>
  <tr>
<td>beqzal	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a == 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>bge	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a >= b</td>
  </tr>
  <tr>
<td>bgeal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a >= b and store next line number in ra</td>
  </tr>
  <tr>
<td>bgez	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a >= 0</td>
  </tr>
  <tr>
<td>bgezal	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a >= 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>bgt	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a > b</td>
  </tr>
  <tr>
<td>bgtal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a > b and store next line number in ra</td>
  </tr>
  <tr>
<td>bgtz	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a > 0</td>
  </tr>
  <tr>
<td>bgtzal	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a > 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>ble	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a <= b</td>
  </tr>
  <tr>
<td>bleal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a <= b and store next line number in ra</td>
  </tr>
  <tr>
<td>blez	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a <= 0</td>
  </tr>
  <tr>
<td>blezal	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a <= 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>blt	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a < b</td>
  </tr>
  <tr>
<td>bltal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a < b and store next line number in ra</td>
  </tr>
  <tr>
<td>bltz	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a < 0</td>
  </tr>
  <tr>
<td>bltzal	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a < 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>bna	a(r?|num)	b(r?|num)	c(r?|num)	d(r?|num)</td>
<td>Branch to line d if abs(a - b) > max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>bnaal	a(r?|num)	b(r?|num)	c(r?|num)	d(r?|num)</td>
<td>Branch to line d if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8) and store next line number in ra</td>
  </tr>
  <tr>
<td>bnaz	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if abs(a) > float.epsilon * 8</td>
  </tr>
  <tr>
<td>bnazal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if abs(a) > float.epsilon * 8</td>
  </tr>
  <tr>
<td>bne	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a != b</td>
  </tr>
  <tr>
<td>bneal	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Branch to line c if a != b and store next line number in ra</td>
  </tr>
  <tr>
<td>bnez	a(r?|num)	b(r?|num)	 	 </td>
<td>branch to line b if a != 0</td>
  </tr>
  <tr>
<td>bnezal	a(r?|num)	b(r?|num)	 	 </td>
<td>Branch to line b if a != 0 and store next line number in ra</td>
  </tr>
  <tr>
<td>brap	a(r?|num)	b(r?|num)	c(r?|num)	d(r?|num)</td>
<td>Relative branch to line d if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>brapz	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Relative branch to line c if abs(a) <= float.epsilon * 8</td>
  </tr>
  <tr>
<td>breq	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Relative branch to line c if a == b</td>
  </tr>
  <tr>
<td>breqz	a(r?|num)	b(r?|num)	 	 </td>
<td>Relative branch to line b if a == 0</td>
  </tr>
  <tr>
<td>brge	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Relative jump to line c if a >= b</td>
  </tr>
  <tr>
<td>brgez	a(r?|num)	b(r?|num)	 	 </td>
<td>Relative branch to line b if a >= 0</td>
  </tr>
  <tr>
<td>brgt	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>relative jump to line c if a > b</td>
  </tr>
  <tr>
<td>brgtz	a(r?|num)	b(r?|num)	 	 </td>
<td>Relative branch to line b if a > 0</td>
  </tr>
  <tr>
<td>brle	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Relative jump to line c if a <= b</td>
  </tr>
  <tr>
<td>brlez	a(r?|num)	b(r?|num)	 	 </td>
<td>Relative branch to line b if a <= 0</td>
  </tr>
  <tr>
<td>brlt	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Relative jump to line c if a < b</td>
  </tr>
  <tr>
<td>brltz	a(r?|num)	b(r?|num)	 	 </td>
<td>Relative branch to line b if a < 0</td>
  </tr>
  <tr>
<td>brna	a(r?|num)	b(r?|num)	c(r?|num)	d(r?|num)</td>
<td>Relative branch to line d if abs(a - b) > max(c * max(abs(a), abs(b)), float.epsilon * 8)</td>
  </tr>
  <tr>
<td>brnaz	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Relative branch to line c if abs(a) > float.epsilon * 8</td>
  </tr>
  <tr>
<td>brne	a(r?|num)	b(r?|num)	c(r?|num)	 </td>
<td>Relative branch to line c if a != b</td>
  </tr>
  <tr>
<td>brnez	a(r?|num)	b(r?|num)	 	 </td>
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
    <td>sapz	r?	a(r?|num)	b(r?|num)	</td>
    <td>Register = 1 if abs(a - b) <= max(c * max(abs(a), abs(b)), float.epsilon * 8), otherwise 0</td>
  </tr>
  <tr>
    <td>sapz	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if |a| <= float.epsilon * 8, otherwise 0</td>
  </tr>
  <tr>
    <td>select	r?	a(r?|num)	b(r?|num)	c(r?|num)</td>
    <td>Register = b if a is non-zero, otherwise c</td>
  </tr>
  <tr>
    <td>seq	r?	a(r?|num)	b(r?|num)	</td>
    <td>Register = 1 if a == b, otherwise 0</td>
  </tr>
  <tr>
    <td>seqz	r?	a(r?|num)	</td>
    <td>Register = 1 if a == 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sge	r?	a(r?|num)	b(r?|num)	</td>
    <td>Register = 1 if a >= b, otherwise 0</td>
  </tr>
  <tr>
    <td>sgez	r?	a(r?|num)</td>
    <td>Register = 1 if a >= 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sgt	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if a > b, otherwise 0</td>
  </tr>
  <tr>
    <td>sgtz	r?	a(r?|num)	</td>
    <td>Register = 1 if a > 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sle	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if a <= b, otherwise 0</td>
  </tr>
  <tr>
    <td>slez	r?	a(r?|num)	 </td>
    <td>Register = 1 if a <= 0, otherwise 0</td>
  </tr>
  <tr>
    <td>slt	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if a < b, otherwise 0</td>
  </tr>
  <tr>
    <td>sltz	r?	a(r?|num)</td>
    <td>Register = 1 if a < 0, otherwise 0</td>
  </tr>
  <tr>
    <td>sna	r?	a(r?|num)	b(r?|num)	c(r?|num)</td>
    <td>Register = 1 if abs(a - b) > max(c * max(abs(a), abs(b)), float.epsilon * 8), otherwise 0</td>
  </tr>
  <tr>
    <td>snaz	r?	a(r?|num)	b(r?|num)	</td>
    <td>Register = 1 if |a| > float.epsilon, otherwise 0</td>
  </tr>
  <tr>
    <td>sne	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if a != b, otherwise 0</td>
  </tr>
  <tr>
    <td>snez	r?	a(r?|num)</td>
    <td>Register = 1 if a != 0, otherwise 0</td>
  </tr>
</table>

## Mathematical

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>abs	r?	a(r?|num)</td>
    <td>Register = the absolute value of a</td>
  </tr>
  <tr>
    <td>acos	r?	a(r?|num)	</td>
    <td>Returns the cosine of the specified angle</td>
  </tr>
  <tr>
    <td>add	r?	a(r?|num)	b(r?|num)	</td>
    <td>Register = a + b.</td>
  </tr>
  <tr>
    <td>asin	r?	a(r?|num)	</td>
    <td>Returns the angle whos sine is the specified value</td>
  </tr>
  <tr>
    <td>atan	r?	a(r?|num)</td>
    <td>Returns the angle whos tan is the specified value</td>
  </tr>
  <tr>
    <td>ceil	r?	a(r?|num)</td>
    <td>Register = smallest integer greater than a</td>
  </tr>
  <tr>
    <td>cos	r?	a(r?|num)</td>
    <td>Returns the cosine of the specified angle</td>
  </tr>
  <tr>
    <td>div	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = a / b</td>
  </tr>
  <tr>
    <td>exp	r?	a(r?|num)</td>
    <td>Register = exp(a)</td>
  </tr>
  <tr>
    <td>floor	r?	a(r?|num)</td>
    <td>Register = largest integer less than a</td>
  </tr>
  <tr>
    <td>log	r?	a(r?|num)</td>
    <td>Register = log(a)</td>
  </tr>
  <tr>
    <td>max	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = max of a or b</td>
  </tr>
  <tr>
    <td>min	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = min of a or b</td>
  </tr>
  <tr>
    <td>mod	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = a mod b (note: NOT a % b)</td>
  </tr>
  <tr>
    <td>mul	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = a * b</td>
  </tr>
  <tr>
    <td>rand	r?</td>
    <td>Register = a random value x with 0 <= x < 1</td>
  </tr>
  <tr>
    <td>round	r?	a(r?|num)</td>
    <td>Register = a rounded to nearest integer</td>
  </tr>
  <tr>
    <td>sin	r?	a(r?|num)</td>
    <td>Returns the sine of the specified angle</td>
  </tr>
  <tr>
    <td>sqrt	r?	a(r?|num)</td>
    <td>Register = square root of a</td>
  </tr>
  <tr>
    <td>sub	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = a - b.</td>
  </tr>
  <tr>
    <td>tan	r?	a(r?|num)	</td>
    <td>Returns the tan of the specified angle</td>
  </tr>
  <tr>
    <td>trunc	r?	a(r?|num)</td>
    <td>Register = a with fractional part removed</td>
  </tr>
</table>

## Logic

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>and	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if a and b not zero, otherwise 0</td>
  </tr>
  <tr>
    <td>nor	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if a and b are 0, otherwise 0</td>
  </tr>
  <tr>
    <td>or	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if a and/or b not 0, otherwise 0</td>
  </tr>
  <tr>
    <td>xor	r?	a(r?|num)	b(r?|num)</td>
    <td>Register = 1 if either a or b not 0, otherwise 0</td>
  </tr>
</table>

## Misc

<table>
  <tr>
    <th>Instruction</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>define	str	num</td>    
    <td>Creates a label that will be replaced throughout the program with the provided value.</td>    
  </tr>
  <tr>
    <td>move	r?	a(r?|num)</td>    
    <td>Register = provided num or register value.</td>    
  </tr>
  <tr>
    <td>yield</td>    
    <td>Pauses execution for 1 tick</td>    
  </tr>
  <tr>
    <td>print r?</td>    
    <td>Print register in the console</td>    
  </tr>
</table>
