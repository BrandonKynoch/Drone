CC=cc

headers_dir = ./headers
global_headers_dir = ../Globals/headers
obj_dir = obj

#Include header dir - this allows us to include headers using <> instead of ""
CFLAGS = -I$(headers_dir) -I$(global_headers_dir) -w # -w suppresses warnings

files = server s_drone_data
header_files = server s_drone_data
global_header_files = globals

objects = $(patsubst %, $(obj_dir)/%.o, $(files))
headers = $(patsubst %, $(headers_dir)/%.h, $(header_files))
global_headers = $(patsubst %, $(global_headers_dir)/%.h, $(global_header_files))

# Make executable
output : $(objects)
	mkdir -p obj
	$(CC) -o output $(objects) $(CFLAGS) -lm

# Make objects
$(obj_dir)/%.o : %.c $(headers) $(global_headers)
	$(CC) -c -o $@ $< $(CFLAGS)

.PHONY : clean
clean :
	rm -r $(obj_dir)
	mkdir $(obj_dir)

.PHONY : remove
remove :
	rm output
