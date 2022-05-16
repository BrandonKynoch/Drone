CC=cc

headers_dir = ./headers
global_headers_dir = ../Globals/headers
obj_dir = obj
global_obj_dir = ../Globals/obj

#Include header dir - this allows us to include headers using <> instead of ""
CFLAGS = -I$(headers_dir) -I$(global_headers_dir)

files = d_master d_networking
global_files = s_drone_data
header_files = d_networking
global_header_files = globals s_drone_data

objects = $(patsubst %, $(obj_dir)/%.o, $(files))
global_objects = $(patsubst %, $(global_obj_dir)/%.o, $(global_files))
headers = $(patsubst %, $(headers_dir)/%.h, $(header_files))
global_headers = $(patsubst %, $(global_headers_dir)/%.h, $(global_header_files))

# Make executable
output : $(objects) $(global_objects)
	mkdir -p obj
	$(CC) -o output $(objects) $(CFLAGS) -lm

# Make objects
$(obj_dir)/%.o : %.c $(headers) $(global_headers)
	$(CC) -c -o $@ $< $(CFLAGS)

$(global_obj_dir)/%.o : %.c $(headers) $(global_headers)
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
