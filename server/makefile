CC=cc

headers_dir = .
obj_dir = obj

#Include header dir - this allows us to include headers using <> instead of ""
CFLAGS = -I$(headers_dir)

files = server
header_files = server

objects = $(patsubst %, $(obj_dir)/%.o, $(files))
headers = $(patsubst %, $(headers_dir)/%.h, $(header_files))

# Make executable
output : $(objects)
	mkdir -p obj
	$(CC) -o output $(objects) $(CFLAGS) -lm

# Make objects
$(obj_dir)/%.o : %.c $(headers)
	$(CC) -c -o $@ $< $(CFLAGS)

.PHONY : clean
clean :
	rm -r $(obj_dir)
	mkdir $(obj_dir)

.PHONY : remove
remove :
	rm output
