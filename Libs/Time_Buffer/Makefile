CC=cc

src_dir = src
headers_dir = headers
obj_dir = obj
lib_dir = lib
output_lib = lib/time_buffer.a

files := $(shell find $(src_dir) -name '*.c')
headers := $(shell find $(headers_dir) -name '*.h')
objects := $(patsubst %.c, $(obj_dir)/%.o, $(notdir $(files)))

CFLAGS = -I$(headers_dir) -w

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
