all:
	cd Drone && $(MAKE) clean && $(MAKE)
	cd ..
	cd Server && $(MAKE) clean && $(MAKE)