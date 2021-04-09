ORG 0
ldi R3, $87
ldi R3, 1(R3)
ld R2, $75
ldi R2, -2(R2)
ld R1, 4(R2)
ldi R0, 1
ldi R3, $73
brmi R3, 3
ldi R3, 5(R3)
ld R7, -3(R3)
nop
brpl R7, 2
ldi R4, 6(R1)
ldi R3, 2(R4)
add R3, R2, R3
addi R7, R7, 3
neg R7, R7
not R7, R7
andi R7, R7, $0F
ori R7, R1, 3
shr R2, R3, R0
st $58, R2
ror R1, R1, R0
rol R2, R2, R0
or R2, R3, R0
and R1, R2, R1
st $67(R1), R2
sub R3, R2, R3
shl R1, R2, R0
ldi R4, 5
ldi R5, $1D
mul R5, R4
mfhi R7
mflo R6
div R5, R4
ldi R10, 0(R4)
ldi R11, 2(R5)
ldi R12, 0(R6)
ldi R13, 0(R7)
jal R12
halt
ORG $91
add R9, R10, R12
sub R8, R11, R13
sub R9, R9, R8
jr R14