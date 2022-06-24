CC=cc

libs_dir = ../Libs

headers_dir = ./headers
obj_dir = obj

global_files_dir = ../Globals
global_headers_dir = ../Globals/headers
global_obj_dir = ../Globals/obj

files := $(shell find . -name '*.c')
headers := $(shell find $(headers_dir) -name '*.h')
objects := $(patsubst %.c, $(obj_dir)/%.o, $(notdir $(files)))

global_files := $(shell find $(global_files_dir) -name '*.c')
global_headers := $(shell find $(global_headers_dir) -name '*.h')
global_objects := $(patsubst %.c, $(global_obj_dir)/%.o, $(notdir $(global_files)))

###################################################################################
#####       LIBRARIES          ####################################################
###################################################################################
lib_Json_C = -ljson-c

lib_BLAS_custom = \
	$(libs_dir)/BLAS_Custom/lib/custom_BLAS.a \
	-I$(libs_dir)/BLAS_Custom/headers

lib_CBLAS = \
	$(libs_dir)/CBLAS/lib/cblas_LINUX.a \
	$(libs_dir)/CBLAS/lib/libblas.a \
	-I$(libs_dir)/CBLAS/include

libraries = $(lib_Json_C) $(lib_BLAS_custom) $(lib_CBLAS)
###################################################################################
#####       LIBRARIES          ####################################################
###################################################################################

CFLAGS = -I$(headers_dir) -I$(global_headers_dir) $(libraries) -w

# Make executable
output : $(objects) $(global_objects)
	mkdir -p obj
	$(CC) -o output $(objects) $(global_objects) $(CFLAGS) -lm

# Make objects
$(obj_dir)/%.o : %.c $(headers) $(global_headers)
	$(CC) -c -o $@ $< $(CFLAGS)

# Make global objects
$(global_obj_dir)/%.o : $(global_files_dir)/%.c $(headers) $(global_headers)
	$(CC) -c -o $@ $< $(CFLAGS)

.PHONY : clean
clean :
	rm -r $(obj_dir)
	mkdir $(obj_dir)
	rm -r $(global_obj_dir)
	mkdir $(global_obj_dir)

.PHONY : remove
remove :
	rm output
