RID ?= linux-x64
BUILD_DIR = ../build/runtimes/$(RID)/native
# Missing Makefile -> razor swift toml tsq
LANGUAGES = agda bash c cpp c-sharp css embedded-template go haskell html java javascript jsdoc json julia ocaml php python ql ruby rust scala typescript tsx verilog
PROJECTS = tree-sitter $(patsubst %,tree-sitter-%,$(LANGUAGES))
LIBRARIES = $(patsubst %,$(BUILD_DIR)/lib%.so,$(PROJECTS))

all: $(BUILD_DIR) $(LIBRARIES)

$(BUILD_DIR):
	mkdir -p $@

$(BUILD_DIR)/libtree-sitter-ocaml.so:
	make -C tree-sitter-ocaml
	cp tree-sitter-ocaml/grammars/ocaml/libtree-sitter-ocaml.so $@

$(BUILD_DIR)/libtree-sitter-php.so:
	make -C tree-sitter-php
	cp tree-sitter-php/php/libtree-sitter-php.so $@

$(BUILD_DIR)/libtree-sitter-typescript.so:
	make -C tree-sitter-typescript
	cp tree-sitter-typescript/typescript/libtree-sitter-typescript.so $@

$(BUILD_DIR)/libtree-sitter-tsx.so:
	make -C tree-sitter-typescript
	cp tree-sitter-typescript/tsx/libtree-sitter-tsx.so $@

$(BUILD_DIR)/lib%.so:
	make -C $*
	cp $*/lib$*.so $@

.PHONY: all
