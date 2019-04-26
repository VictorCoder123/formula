grammar Formula;

/*
 * Parser Rules
 */

compileUnit
	:	EOF
	;

/*
 * Lexer Rules
 */

OPERATOR 
	: ':-' | '-->'
	;

WS
	//:	' ' -> channel(HIDDEN)
	: [ \t\r\n]+ -> channel(HIDDEN)
	;
