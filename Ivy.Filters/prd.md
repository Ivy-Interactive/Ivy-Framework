
# A) Scope & semantics (what we parse)

**Goal:** Parse the user’s *formula* text into a typed AST you can validate.

**Features covered**

* Column refs in **square brackets** using the header display name: `[Age]`, `[Country]`.
* Logical joins: `AND`, `OR`, parentheses.
* **Text ops:** `contains`, `not contains`, `equals`, `not equal`, `starts with`, `ends with`, `blank`, `not blank`. (Case-insensitive keywords.)
* **Number ops:** `=`, `==`, `!=`, `>`, `>=`, `<`, `<=`, plus word forms `equals`, `not equal`, `greater than`, `greater than or equal`, etc., and **`in range`** (two operands).
* **Date/DateTime ops:** `equals`, `not equal`, `before` (`<`), `after` (`>`), **`in range`**. Inputs are strings; your evaluator parses to `DateTime`. Inclusivity is configurable like Date Filter.
* String literals: **double-quoted** with escapes: `"united"`, `"he said \"hi\""`.

**What this grammar deliberately doesn’t decide**

* Type inference. You supply a column metadata map `{ displayName -> type }` (text/number/date/datetime/bool).
* Value parsing (e.g., dates). You plug a parser/comparator consistent with your grid config. * The exact *model* shape; we output an AST you’ll lower to the grid’s **join/leaf** model (`filterType`, `type`, `conditions`, etc.). 
---

# B) Operators (normalized names & arity)

These are the **normalized** operator ids your visitor should emit (you’ll map synonyms to these). The sets reflect the the grid provided filters. 
## Text

* `contains(s)` / `notContains(s)` – 1 operand (string)
* `equals(s)` / `notEqual(s)` – 1 operand (string)
* `startsWith(s)` / `endsWith(s)` – 1 operand (string)
* `blank()` / `notBlank()` – 0 operands

## Number

* `equals(n)`, `notEqual(n)`, `greaterThan(n)`, `greaterThanOrEqual(n)`, `lessThan(n)`, `lessThanOrEqual(n)` – 1 numeric operand
* `inRange(n1, n2)` – 2 numeric operands (inclusive/exclusive at eval time per your config) * `blank()`, `notBlank()` – 0 operands

## Date / DateTime

* `equals(d)`, `notEqual(d)`, `lessThan(d)`(Before), `greaterThan(d)`(After) – 1 date operand
* `inRange(d1, d2)` – 2 date operands; inclusivity configurable like the grid’s `inRangeInclusive` * `blank()`, `notBlank()` – 0 operands

**Synonyms accepted by the grammar** (map to normalized ids):

* `=`/`==` → `equals`, `!=` → `notEqual`
* `>`/`>=`/`<`/`<=` and word forms: “greater than or equal”, “less than”, etc.
* `in range`, `inRange` (both accepted)
* “not contains” → `notContains`; “not equal(s)” → `notEqual`
* Text multi-word operators: “starts with”, “ends with”

---

# C) ANTLR 4 grammar (AdvancedFilter.g4)

> Save as **AdvancedFilter.g4** and generate with:
> `antlr4 -Dlanguage=CSharp AdvancedFilter.g4`
> (You’ll add a custom error listener & a visitor—see section D.)

```antlr
// AdvancedFilter.g4
// ANTLR 4 grammar for the grid–style Advanced Filter formulas.
// Accepts [Column] operators, AND/OR, parentheses, strings, numbers, and date-likes as strings.
// Keywords are case-insensitive via explicit character classes.

grammar AdvancedFilter;

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
  : NOT? primary
  ;

primary
  : group
  | comparison
  | textOperation
  | rangeOperation
  | existenceOperation
  ;

group
  : LPAREN expr RPAREN
  ;

// [Column] <compOp> <operand>
// Symbolic ops (=, ==, !=, >, >=, <, <=) or word forms (equals, greater than, etc.)
comparison
  : fieldRef compOp operand
  ;

// For text-specific "contains/starts with/ends with" style operators
textOperation
  : fieldRef textOp stringLiteral
  ;

// in range: [Col] in range <v1> AND <v2>
// Works for numbers and dates (operands parsed as number or date later)
rangeOperation
  : fieldRef IN RANGE operand AND operand
  | fieldRef INRANGE operand AND operand            // allow camelCase "inRange"
  ;

// blank / not blank (no operand)
existenceOperation
  : fieldRef BLANK
  | fieldRef NOT BLANK
  ;

// --------------------
// Operands & fields
// --------------------

operand
  : number
  | stringLiteral
  ;

// [ Display Name ]
fieldRef
  : LBRACK identifier RBRACK
  ;

// --------------------
// Operators
// --------------------

// Symbolic and word forms normalized by the visitor

compOp
  : EQ
  | EQ2
  | NEQ
  | GT
  | GE
  | LT
  | LE
  | EQUALS
  | NOTEQUAL
  | GREATER THAN
  | GREATER THANOR EQUAL
  | GREATER THANOREQUAL
  | GREATEROREQUAL        // "greater or equal"
  | GREATERTHAN
  | GREATERTHANOREQUAL
  | LESS THAN
  | LESS THANOR EQUAL
  | LESS THANOREQUAL
  | LESSOREQUAL
  | LESSTHAN
  | LESSTHANOREQUAL
  ;

textOp
  : CONTAINS
  | NOT CONTAINS
  | STARTS WITH
  | ENDS WITH
  ;

// --------------------
// Lexical elements
// --------------------

number
  : SIGN? DIGITS (DOT DIGITS)?
  ;

stringLiteral
  : DQUOTE stringChar* DQUOTE
  ;

identifier
  : IDENT_START IDENT_CONT*
  ;

// --------------------
// Tokens (keywords are case-insensitive)
// --------------------

// Logical
AND : [Aa][Nn][Dd] ;
OR  : [Oo][Rr] ;
NOT : [Nn][Oo][Tt] ;

// Text ops
CONTAINS : [Cc][Oo][Nn][Tt][Aa][Ii][Nn][Ss] ;
STARTS   : [Ss][Tt][Aa][Rr][Tt][Ss] ;
ENDS     : [Ee][Nn][Dd][Ss] ;
WITH     : [Ww][Ii][Tt][Hh] ;

// Existence
BLANK    : [Bb][Ll][Aa][Nn][Kk] ;

// Range
IN       : [Ii][Nn] ;
RANGE    : [Rr][Aa][Nn][Gg][Ee] ;
INRANGE  : [Ii][Nn][Rr][Aa][Nn][Gg][Ee] ; // camelCase variant

// Wordy comparison
EQUALS     : [Ee][Qq][Uu][Aa][Ll][Ss]? ;
NOTEQUAL   : [Nn][Oo][Tt][ \t]*[Ee][Qq][Uu][Aa][Ll][Ss]? ;
GREATER    : [Gg][Rr][Ee][Aa][Tt][Ee][Rr] ;
LESS       : [Ll][Ee][Ss][Ss] ;
THAN       : [Tt][Hh][Aa][Nn] ;
OREQUAL    : [Oo][Rr][ \t]*[Ee][Qq][Uu][Aa][Ll] ;

// Punctuation & symbolic ops
LPAREN : '(' ;
RPAREN : ')' ;
LBRACK : '[' ;
RBRACK : ']' ;
DQUOTE : '"' ;

EQ  : '=' ;
EQ2 : '==' ;
NEQ : '!=' ;
GT  : '>' ;
GE  : '>=' ;
LT  : '<' ;
LE  : '<=' ;

DOT   : '.' ;
SIGN  : [+\-] ;
DIGITS: [0-9]+ ;

// IDENT (allow spaces to match the grid display names inside [ ... ])
IDENT_START : [A-Za-z0-9_] ;
IDENT_CONT  : [A-Za-z0-9_ ] ;

// String contents with escapes for \" and \\
// (You can extend this with \n \t if desired.)
fragment ESCAPED_QUOTE : '\\"' ;
fragment ESCAPED_BS    : '\\\\' ;
fragment STRING_CHAR   : ~["\\] | ESCAPED_QUOTE | ESCAPED_BS ;
stringChar : STRING_CHAR ;

// Whitespace & comments
WS : [ \t\r\n]+ -> skip ;
LINE_COMMENT : '//' ~[\r\n]* -> skip ;
BLOCK_COMMENT: '/*' .*? '*/' -> skip ;

// --------------------
// Helper tokens used in compOp composition
// --------------------
THANOR : THAN [ \t]* OREQUAL ;      // "than or equal"
GREATEROREQUAL : GREATER [ \t]* OREQUAL ;
GREATERTHAN    : GREATER [ \t]* THAN ;
GREATERTHANOREQUAL : GREATER [ \t]* THANOR ;
LESSOREQUAL    : LESS [ \t]* OREQUAL ;
LESSTHAN       : LESS [ \t]* THAN ;
LESSTHANOREQUAL: LESS [ \t]* THANOR ;
```

### Notes on the grammar

* **Case-insensitivity**: keywords are matched with `[Aa]` style classes.
* **Multi-word ops**: `STARTS WITH`, `ENDS WITH`, and the composed comparisons like `GREATER THAN OR EQUAL` are modeled as sequences, then normalized in your visitor.
* **`in range`**: Both `"in range"` and `"inRange"` are accepted:
  `[Age] in range 18 AND 30` or `[Date] inRange "2024-01-01" AND "2024-12-31"`.
* **Strings**: `"..."` with escapes `\"` and `\\`. If you need more escapes, extend `STRING_CHAR`.
* **Identifiers in [ ]**: allow spaces to match the grid display names (e.g., `[Start Date]`). You’ll provide a mapping to `colId` later.
* **Symbolic vs word comparisons**: both forms accepted; your visitor maps them to normalized operator ids (see section B).

---

# D) C# integration & validation

## 1) Generate parser/lexer

```bash
antlr4 -Dlanguage=CSharp AdvancedFilter.g4
# produces AdvancedFilterLexer.cs, AdvancedFilterParser.cs, etc.
```

## 2) Column schema & type environment

Provide to the evaluator:

```csharp
enum ColType { Text, Number, Date, DateTime, Boolean }
record ColumnMeta(string DisplayName, string ColId, ColType Type);

IDictionary<string,ColumnMeta> ColumnsByDisplayName; // key = display name in [ ... ]
```

## 3) AST model

Create a simple, serializable AST to later lower into the grid’s model:

```csharp
abstract record Node;
record And(Node Left, Node Right) : Node;
record Or(Node Left, Node Right) : Node;
record Not(Node Inner) : Node;

enum Op {
  Contains, NotContains, StartsWith, EndsWith,
  Equals, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual,
  InRange, Blank, NotBlank
}

record Leaf(string ColDisplay, string ColId, ColType Type, Op Op, object? A = null, object? B = null) : Node;
```

## 4) Visitor to build AST + normalize operators

Implement `AdvancedFilterBaseVisitor<Node>` and:

* Resolve `[Display Name]` → `ColumnMeta` (error if unknown/duplicate).
* **Normalize** operator tokens:

    * Symbolic `=`, `==` → `Equals`; `!=` → `NotEqual`; `>` → `GreaterThan`, etc.
    * Word forms (`GREATER THAN`, `LESS THAN OR EQUAL`, etc.) → corresponding enum.
    * Text ops: `CONTAINS`, `NOT CONTAINS`, `STARTS WITH`, `ENDS WITH`.
* **Operands**:

    * For numbers: parse `decimal` (or `double`) respecting culture (“.” decimal expected from grammar).
    * For dates: keep raw string here; your evaluator parses via `DateTime.TryParse` or a configured parser consistent with the grid’s date filter settings (e.g., `inRangeInclusive`). * **Arity/type validation**: e.g., `inRange` must have two operands; `blank` has none; text ops require `ColType.Text`; `>` on Text is invalid; raise a diagnostic.

## 5) Diagnostics (developer-friendly)

Attach a **custom error listener** to both lexer and parser so syntax errors return precise messages (unterminated string, missing `]`, etc.). For semantic errors (e.g., “`contains` used on Number column”), collect into a list with source spans (use `ctx.Start.Line/Column`).

## 6) Lowering to the grid’s Advanced Filter Model

Convert the AST to the grid’s **join/leaf** JSON model (ready for `gridApi.setAdvancedFilterModel(model)`):

* `And/Or/Not` → nested **join** nodes (`{ filterType: 'join', type: 'AND'|'OR', conditions: [...] }`). For `Not`, wrap the child in a special negation (the grid advanced filter UI supports NOT via condition structure; when not directly supported, flip the leaf operator to its negation where possible).
* `Leaf` → `{ filterType: 'text'|'number'|'date'|'dateTime'|'boolean', colId, type, filter?, filterTo? }`

    * For `inRange`, populate both `filter` and `filterTo`.
    * Map enum `Op` to the grid’s operator strings (`contains`, `notContains`, `equals`, `greaterThan`, etc.) per the provided filter docs. * You can then feed this into the grid’s API. 
---

# E) Test corpus (quick confidence suite)

**Valid**

1. `[Age] > 23`
2. `[Sport] ends with "ing"`
3. `([Age] > 23 OR [Sport] ends with "ing") AND [Country] contains "united"`
4. `[Price] in range 10 AND 20`
5. `[Start Date] inRange "2024-01-01" AND "2024-12-31"`
6. `[Country] blank`
7. `[Name] not contains "x\"y"` (escaped quote)

**Invalid (semantic)**

1. `[Age] contains "12"` → `contains` on Number
2. `[Date] > "yesterday"` (if your date parser disallows non-ISO text)
3. `[X] in range 10 AND` (missing second operand)

**Invalid (syntax)**

1. `[Age > 23` (missing `]`)
2. `"abc` (unterminated string)
3. `([A] = 1 OR )` (dangling join)

---
