grammar FilterQuery;

// Parser Rules
query
    : expression EOF
    ;

expression
    : '(' expression ')'                            # ParenExpression
    | NOT expression                                # NotExpression
    | expression AND expression                     # AndExpression
    | expression OR expression                      # OrExpression
    | condition                                     # ConditionExpression
    ;

condition
    : columnName EQUALS value                       # EqualsCondition
    | columnName NOT_EQUALS value                   # NotEqualsCondition
    | columnName GREATER_THAN value                 # GreaterThanCondition
    | columnName GREATER_THAN_EQUALS value          # GreaterThanEqualsCondition
    | columnName LESS_THAN value                    # LessThanCondition
    | columnName LESS_THAN_EQUALS value             # LessThanEqualsCondition
    | columnName CONTAINS value                     # ContainsCondition
    | columnName STARTS_WITH value                  # StartsWithCondition
    | columnName ENDS_WITH value                    # EndsWithCondition
    | columnName IN '(' valueList ')'               # InCondition
    | columnName NOT IN '(' valueList ')'           # NotInCondition
    | columnName IS NULL_LITERAL                    # IsNullCondition
    | columnName IS NOT NULL_LITERAL                # IsNotNullCondition
    ;

columnName
    : IDENTIFIER
    | QUOTED_IDENTIFIER
    ;

value
    : STRING_LITERAL
    | NUMBER_LITERAL
    | BOOLEAN_LITERAL
    | NULL_LITERAL
    ;

valueList
    : value (',' value)*
    ;

// Lexer Rules

// Keywords
AND             : 'AND' | 'and' ;
OR              : 'OR' | 'or' ;
NOT             : 'NOT' | 'not' ;
IN              : 'IN' | 'in' ;
IS              : 'IS' | 'is' ;
CONTAINS        : 'CONTAINS' | 'contains' ;
STARTS_WITH     : 'STARTS_WITH' | 'starts_with' | 'STARTSWITH' | 'startswith' ;
ENDS_WITH       : 'ENDS_WITH' | 'ends_with' | 'ENDSWITH' | 'endswith' ;

// Operators
EQUALS                  : '=' | '==' ;
NOT_EQUALS              : '!=' | '<>' ;
GREATER_THAN            : '>' ;
GREATER_THAN_EQUALS     : '>=' ;
LESS_THAN               : '<' ;
LESS_THAN_EQUALS        : '<=' ;

// Literals
BOOLEAN_LITERAL
    : 'true' | 'TRUE' | 'false' | 'FALSE'
    ;

NULL_LITERAL
    : 'null' | 'NULL'
    ;

STRING_LITERAL
    : '"' (~["\r\n] | '""')* '"'
    | '\'' (~['\r\n] | '\'\'')* '\''
    ;

NUMBER_LITERAL
    : '-'? [0-9]+ ('.' [0-9]+)?
    ;

IDENTIFIER
    : [a-zA-Z_][a-zA-Z0-9_]*
    ;

QUOTED_IDENTIFIER
    : '[' ~[\]]+ ']'
    | '`' ~[`]+ '`'
    ;

// Whitespace
WS
    : [ \t\r\n]+ -> skip
    ;
