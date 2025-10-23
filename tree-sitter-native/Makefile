RID ?= linux-x64
BUILD_DIR = ../build/runtimes/$(RID)/native

ifeq ($(OS),Windows_NT)
    make_lib = $(addsuffix .dll,$1)
else
    UNAME_S := $(shell uname -s)
    ifeq ($(UNAME_S),Darwin)
        make_lib = $(addsuffix .dylib,$1)
    else
        make_lib = $(addsuffix .so,$1)
    endif
endif

# Missing Makefile or broken for Linux/OSX -> ocaml razor swift toml tsq
LANGUAGES = agda bash c cpp c-sharp css embedded-template go haskell html java javascript jsdoc json julia php python ql ruby rust scala typescript tsx verilog
PROJECTS = tree-sitter $(patsubst %,tree-sitter-%,$(LANGUAGES))
LIBRARIES = $(patsubst %,$(BUILD_DIR)/$(call make_lib,lib%),$(PROJECTS))

all: $(BUILD_DIR) $(LIBRARIES)

$(BUILD_DIR):
	mkdir -p $@

$(BUILD_DIR)/$(call make_lib,libtree-sitter-ocaml):
	make -C tree-sitter-ocaml
	cp tree-sitter-ocaml/grammars/ocaml/libtree-sitter-ocaml.so $@

$(BUILD_DIR)/$(call make_lib,libtree-sitter-ocaml-type):
	make -C tree-sitter-ocaml
	cp tree-sitter-ocaml/grammars/type/libtree-sitter-ocaml-type.so $@

$(BUILD_DIR)/$(call make_lib,libtree-sitter-php):
	make -C tree-sitter-php
	cp tree-sitter-php/php/$(call make_lib,libtree-sitter-php) $@

$(BUILD_DIR)/$(call make_lib,libtree-sitter-typescript):
	make -C tree-sitter-typescript
	cp tree-sitter-typescript/typescript/$(call make_lib,libtree-sitter-typescript) $@

$(BUILD_DIR)/$(call make_lib,libtree-sitter-tsx):
	make -C tree-sitter-typescript
	cp tree-sitter-typescript/tsx/$(call make_lib,libtree-sitter-tsx) $@

$(BUILD_DIR)/$(call make_lib,lib%):
	make -C $*
	cp $*/$(call make_lib,lib$*) $@

.PHONY: all
