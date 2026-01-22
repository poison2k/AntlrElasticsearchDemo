grammar BoolQuery;

query: orExpr EOF;

orExpr 
    : andExpr  (OR andExpr)*;

andExpr
    : term (AND term)*;

term
    : NOT? factor;

factor
    : WORD
    | PHRASE
    | nearCall
    | LPAREN orExpr RPAREN ;

nearCall
    : NEAR LPAREN factor COMMA factor (SEMI NUMBER)? RPAREN;
    
AND: A N D; // case-insensitive via Lexer-Regeln unten
OR : O R;
NOT: N O T;

NEAR: N E A R;

// Token
LPAREN: '(';
RPAREN: ')';
COMMA: ',';
SEMI: ';';

NUMBER: [0-9]+;

PHRASE: '"' (~["\\\r\n])* '"' ;
WORD: [\p{L}\p{N}_\-]+;
WS: [ \t\r\n]+ -> skip;

fragment A: [aA];
fragment N: [nN];
fragment D: [dD];
fragment O: [oO];
fragment R: [rR];
fragment T: [tT];
fragment E: [eE];