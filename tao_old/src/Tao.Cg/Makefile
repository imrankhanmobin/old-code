
DEPTH = ../..

include $(DEPTH)/config.mk

LIBRARY = Tao.Cg
DOCS = 1
POSTPROCESS_LIBRARY = 1

SRCS = \
	AssemblyInfo.cs \
	Cg.cs \
	CgGl.cs \
	DelegateCallingConventionCdeclAttribute.cs \
	$(NULL)

CSFLAGS += /unsafe /doc:$(DOC_DEST)/$(LIBRARY).xml

include $(DEPTH)/rules.mk
