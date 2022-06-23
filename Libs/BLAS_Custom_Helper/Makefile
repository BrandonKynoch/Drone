CC=cc

src_dir = src
headers_dir = headers
obj_dir = obj
lib_dir = lib
output_lib = lib/custom_BLAS.a

files := $(shell find $(src_dir) -name '*.c')
headers := $(shell find $(headers_dir) -name '*.h')
objects := $(patsubst %.c, $(obj_dir)/%.o, $(notdir $(files)))

###################################################################################
#####       LIBRARIES          ####################################################
###################################################################################
lib_CBLAS = \
		../CBLAS/lib/cblas_LINUX.a \
		../CBLAS/lib/libblas.a \
		-I../CBLAS/include

libraries = $(lib_CBLAS)
###################################################################################
#####       LIBRARIES          ####################################################
###################################################################################

CFLAGS = -I$(headers_dir) $(libraries) -w

# Build static library
lib : $(objects)
	$ ar -rc $(output_lib) $(objects)

# Make objects
$(obj_dir)/%.o : $(src_dir)/%.c $(headers)
	$(CC) -c -o $@ $< $(CFLAGS)




.PHONY : exe
exe : $(objects)
	mkdir -p obj
	$(CC) -o output $(objects) $(CFLAGS) -lm

.PHONY : clean
clean :
	rm -r $(obj_dir)
	mkdir $(obj_dir)
	rm -r $(lib_dir)
	mkdir $(lib_dir)

.PHONY : remove
remove :
	rm output

.PHONY : libs
libs:
	$(MAKE) -C ../CBLAS
