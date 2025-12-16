grammar BoolQuery;

query: expr EOF;

expr
: expr OR term # OrExpr
| expr AND term # AndExpr
| term # ToTerm
;

term
: NOT? factor # NotTerm
;

factor
: WORD # Word
| LPAREN expr RPAREN # Paren
;

AND: A N D; // case-insensitive via Lexer-Regeln unten
OR : O R;
NOT: N O T;

LPAREN: '(';
RPAREN: ')';

WORD: [\p{L}\p{N}_\-]+; // Worte/Token
WS: [ \t\r\n]+ -> skip;

fragment A: [aA];
fragment N: [nN];
fragment D: [dD];
fragment O: [oO];
fragment R: [rR];
fragment T: [tT];