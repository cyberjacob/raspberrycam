#CXXFLAGS =	-O2 -g -Wall -fmessage-length=0
CFLAGS  = -g -O2 -DHAVE_CONFIG_H
LDFLAGS = -lgd


OBJS =		RaspberryCam.o log.o src.o src_file.o src_raw.o src_test.o src_v4l1.o src_v4l2.o 
OBJS +=		parse.o dec_jpeg.o

LIBS =

TARGET =	RaspberryCam

$(TARGET):	$(OBJS)
##	$(CXX) -o $(TARGET) $(OBJS) $(LIBS)
	$(CC) -o $(TARGET) $(OBJS) $(LIBS) $(LDFLAGS)

all:	$(TARGET)

clean:
	rm -f $(OBJS) $(TARGET) grab.raw
