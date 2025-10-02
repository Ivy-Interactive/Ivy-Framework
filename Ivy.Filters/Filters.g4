// Filters.g4
// ANTLR 4 grammar for grid–style Advanced Filter formulas

grammar Filters;

// --------------------
// Parser rules
// --------------------

formula
  : expr EOF
  ;

expr
  : orExpr
  ;

orExpr
  : andExpr (OR andExpr)*
  ;

andExpr
  : unaryExpr (AND unaryExpr)*
  ;

unaryExpr
  : NOT unaryExpr
  | primary
  ;

primary
  : group
  | comparison
  | textOperation
  | existenceOperation
  ;

group
  : LPAREN expr RPAREN
  ;

// Comparison operations
comparison
  : fieldRef compOp operand
  ;

// Text operations
textOperation
  : fieldRef textOp stringLiteral
  | fieldRef NOT textOp stringLiteral
  ;

// Existence operations
existenceOperation
  : fieldRef IS BLANK
  | fieldRef IS NOT BLANK
  ;

// Field reference - uses special field token
fieldRef
  : FIELD
  ;

// Field identifier extracted from FIELD token
fieldIdentifier
  : // This is handled in the lexer as FIELD token
  ;

// Operators
compOp
  : EQ
  | EQ2
  | NEQ
  | GT
  | GE
  | LT
  | LE
  | EQUALS
  | NOT EQUALS
  | NOT EQUAL
  | GREATER THAN
  | GREATER THAN OR EQUAL
  | LESS THAN
  | LESS THAN OR EQUAL
  ;

textOp
  : CONTAINS
  | STARTS WITH
  | ENDS WITH
  ;

// Operands
operand
  : number
  | stringLiteral
  | booleanLiteral
  ;

booleanLiteral
  : TRUE
  | FALSE
  ;

number
  : SIGN? DIGITS (DOT DIGITS)?
  ;

stringLiteral
  : STRING
  ;

// --------------------
// Lexer tokens - Order matters!
// --------------------

// Field reference - special handling for bracketed field names
FIELD : '[' (~[\r\n\]])+ ']' ;

// String literal with escape sequences
STRING
  : '"' ( '\\' . | ~[\\"\r\n] )* '"'
  ;

// Keywords (case-insensitive) - Order matters! Longer tokens first
CONTAINS : [Cc][Oo][Nn][Tt][Aa][Ii][Nn][Ss] ;
GREATER  : [Gg][Rr][Ee][Aa][Tt][Ee][Rr] ;
STARTS   : [Ss][Tt][Aa][Rr][Tt][Ss] ;
EQUALS   : [Ee][Qq][Uu][Aa][Ll][Ss] ;
EQUAL    : [Ee][Qq][Uu][Aa][Ll] ;
BLANK    : [Bb][Ll][Aa][Nn][Kk] ;
LESS     : [Ll][Ee][Ss][Ss] ;
THAN     : [Tt][Hh][Aa][Nn] ;
ENDS     : [Ee][Nn][Dd][Ss] ;
WITH     : [Ww][Ii][Tt][Hh] ;
NOT      : [Nn][Oo][Tt] ;
OR       : [Oo][Rr] ;
IS       : [Ii][Ss] ;

// Logical AND
AND      : [Aa][Nn][Dd] ;

// Boolean literals
TRUE     : [Tt][Rr][Uu][Ee] ;
FALSE    : [Ff][Aa][Ll][Ss][Ee] ;

// Punctuation
LPAREN : '(' ;
RPAREN : ')' ;

// Symbolic operators
EQ  : '=' ;
EQ2 : '==' ;
NEQ : '!=' ;
GT  : '>' ;
GE  : '>=' ;
LT  : '<' ;
LE  : '<=' ;

// Numbers
DOT   : '.' ;
SIGN  : [+\-] ;
DIGITS: [0-9]+ ;

// Whitespace
WS : [ \t\r\n]+ -> skip ;