FROM fluent/fluentd:v1.14-debian-1

# --------------------
# user root
USER root

# install command
RUN apt-get update && apt-get install -y --no-install-recommends \
        vim \
        unzip \
        git \
        wget

# change bash
RUN mv /bin/sh /bin/sh_tmp && ln -s /bin/bash /bin/sh

# set timezone
ARG TZ="Asia/Tokyo"
ENV TZ ${TZ}
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && \
        echo $TZ > /etc/timezone

# set locale
RUN apt-get update
RUN apt-get install -y locales
RUN echo "ja_JP.UTF-8 UTF-8" > /etc/locale.gen && \
    locale-gen ja_JP.UTF-8

# --------------------
# user fluent
USER fluent

# copy configuration
COPY ./etc/fluent.conf /fluentd/etc/fluent.conf
