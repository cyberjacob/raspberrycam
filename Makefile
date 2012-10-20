CFLAGS  = -g -O2 -DHAVE_CONFIG_H
LDFLAGS = -lgd

OBJS =		RaspberryCam.o log.o src.o src_file.o src_raw.o src_test.o src_v4l1.o src_v4l2.o 
OBJS +=		parse.o dec_jpeg.o

LIBS =

TARGET =	RaspberryCam

$(TARGET):	$(OBJS)
	$(CC) -shared -fPIC $(OBJS) $(LIBS) $(LDFLAGS) -o $(TARGET).so

all:	$(TARGET)

clean:
	rm -f $(OBJS) $(TARGET) grab.raw $(TARGET).so

install:
	cp *.so /lib
	