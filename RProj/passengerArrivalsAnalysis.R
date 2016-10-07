#Fitting a distribution for the amount of passengers per hour.
#This average per hour can then be used as the rate parameter to generate
#interarrival times via a Poisson proces.

# The bus data going from AZU, Heidelberglaan, De Kromme Rijn, Stadion Galgenwaard,
#                         Rubenslaan, Sterrenwijk, Bleekstraat to CS Centrumzijde.
bus_AZU_Centraal <- read.csv("12a.csv",
                              header=TRUE, sep=";")

# The bus data going from CS Centrumzijde, Bleekstraat, Sterrenwijk, Rubenslaan,
#                         Stadion Galgenwaard, De Kromme Rijn, Heidelberglaan to AZU.
bus_Centraal_AZU <- read.csv("12b.csv",
                              header=TRUE, sep=";")


# Remove unnecessary Trip information.
bus_AZU_Centraal <- subset(bus_AZU_Centraal, select=-Trip)
bus_Centraal_AZU <- subset(bus_Centraal_AZU, select=-Trip)


# Sum all passenger arrivals within an hour per day.
bus_AZU_Centraal <- aggregate(x = bus_AZU_Centraal[colnames(bus_AZU_Centraal[,3:ncol(bus_AZU_Centraal)])],
                              FUN = sum,
                              by = list(Date = bus_AZU_Centraal$Date,
                                        Time = as.POSIXlt(strptime(bus_AZU_Centraal$Departure, format="%H:%M"))$hour))

# https://stackoverflow.com/questions/13649019/with-r-split-time-series-data-into-time-intervals-say-an-hour-and-then-plot-t


# First sort by time and by date to have an ordering of days and their hours.
# https://stackoverflow.com/questions/10683224/obtain-hour-from-datetime-vector
bus_AZU_Centraal <- bus_AZU_Centraal[order(as.Date(bus_AZU_Centraal$Date, format="%d-%m-%Y"),
                                           as.POSIXlt(bus_AZU_Centraal$Time, format="%H:%M"), drop=FALSE)]

#
# # Sum all passenger arrivals within an hour per day.
 bus_AZU_Centraal <- aggregate(x = bus_AZU_Centraal[colnames(bus_AZU_Centraal[,3:ncol(bus_AZU_Centraal)])],
                               FUN = sum,
                               by = list(Date = bus_AZU_Centraal$Date,
                                         Hour = cut(as.POSIXct(bus_AZU_Centraal$Departure), format="%H%M", "hour")))

# # https://stackoverflow.com/questions/13649019/with-r-split-time-series-data-into-time-intervals-say-an-hour-and-then-plot-t
#
#
# # First sort by time and by date to have an ordering of days and their hours.
# # https://stackoverflow.com/questions/10683224/obtain-hour-from-datetime-vector
 bus_AZU_Centraal <- bus_AZU_Centraal[order(as.Date(bus_AZU_Centraal$Date,format="%d-%m-%Y"),
                                            as.POSIXlt(bus_AZU_Centraal$Hour, format="%H%M")),,drop=FALSE]


# Remove unnecessary row names.
row.names(bus_AZU_Centraal) <- NULL




# The tram prognosis data in both directions.
 tram <- read.csv("passengerprognose.csv",
                  header= FALSE, sep=";")





# # Distribution fitting
# # Create 200 data points from normal distribution.
# x.norm <- rnorm(n=200, m=10, sd=2)
# # Plot data points.
# hist(x.norm, main="Histogram of observed data")
# 
# # Plot density of data.
# plot(density(x.norm), main="Density estimate of data")
# plot(ecdf(x.norm), main="Empirical cumulative distribution function")
# 
# # Create QQPlot.
# z.norm <- (x.norm-mean(x.norm)) / sd(x.norm) ## standardized data
# qqnorm(z.norm) ## drawing the QQplot
# abline(0, 1) ## drawing a 45-degree reference line
# 
# 
# # Weibull QQPlot.
# x.wei <- rweibull(n=200, shape=2.1, scale=1.1) ## sampling from a Weibull distribution with parameters shape=2.1 and scale=1.1
# x.teo <- rweibull(n=200, shape=2, scale=1) ## theorical quantiles from a Weibull population with known parameters shape=2 & scale=1
# qqplot(x.teo, x.wei, main="QQ-plot distr. Weibull") ## QQ-plot
# abline(0, 1) ## a 45-degree reference line is plotted
# 
# 
# # Poisson distribution
# x.poi<-rpois(n=200, lambda=2.5)
# hist(x.poi, main="Poisson distribution")
# # Normal distribution
# curve(dnorm(x, m=10, sd=2), from=0, to=20, main="Normal distribution")
# # Gamma distribution
# curve(dgamma(x, scale=1.5, shape=2),from=0, to=15, main="Gamma distribution")
# # Weibull
# curve(dweibull(x, scale=2.5, shape=1.5),from=0, to=15, main="Weibull distribution")
# 
# #install.packages("fBasics")
# library(fBasics) ## package loading
# skewness(x.norm) ## skewness of a normal distribution
# kurtosis(x.norm) ## kurtosis of a normal distribution
# skewness(x.wei) ## skewness of a Weibull distribution
# kurtosis(x.wei) ## kurtosis of a Weibull distribution
# 
# 
# # Get the gamma parameters lambda and alpha.
# x.gam <- rgamma(200, rate=0.5, shape=3.5) ## sampling from a gamma distribution with ??=0.5 (scale parameter 12) and ??=3.5 (shape parameter)
# med.gam <- mean(x.gam) ## sample mean
# var.gam <- var(x.gam) ## sample variance
# l.est <- med.gam/var.gam ## lambda estimate (corresponds to rate)
# a.est <- ((med.gam)^2)/var.gam ## alfa estimate
# 
# # Fit distributions
# #install.packages("MASS")
# library(MASS) ## loading package MASS
# fitdistr(x.gam, "gamma") ## fitting gamma pdf parameters
# fitdistr(x.wei, densfun=dweibull, start=list(scale=1, shape=2))## fitting Weibull pdf parameters
# fitdistr(x.norm,"normal") ## fitting gaussian pdf parameters
# 
# 
# ## -------------- Goodness of Fit --------------
# lambda.est <- mean(x.poi) ## estimate of parameter lambda
# tab.os <- table(x.poi)## table with empirical frequencies
# freq.os <- vector()
# for(i in 1: length(tab.os)) freq.os[i]<-tab.os[[i]] ## vector of empirical frequencies
# freq.ex<-(dpois(0:max(x.poi),lambda=lambda.est)*200) ## vector of fitted (expected) frequencies
# acc <- mean(abs(freq.os-trunc(freq.ex))) ## absolute goodness of fit index
# acc/mean(freq.os)*100 ## relative (percent) goodness of fit index
# 
# # The pdf curve and histogram plotted together.
# h <- hist(x.norm, breaks=15)
# xhist <- c(min(h$breaks), h$breaks)
# yhist <- c(0, h$density, 0)
# xfit <- seq(min(x.norm), max(x.norm), length=40)
# yfit <- dnorm(xfit, mean=mean(x.norm), sd=sd(x.norm))
# plot(xhist, yhist, type="s", ylim=c(0,max(yhist, yfit)), main="Normal pdf and
#      histogram")
# lines(xfit, yfit, col="red")
# 
# # Chi-square goodness test
# #install.packages("vcd")
# library(vcd) ## loading vcd package
# gf <- goodfit(x.poi, type="poisson", method="MinChisq")
# summary(gf)
# plot(gf, main="Count data vs Poisson distribution")
# 
# # Continous distribution chi-square test
# x.gam.cut<-cut(x.gam,breaks=c(0,3,6,9,12,18)) ##binning data
# table(x.gam.cut) ## binned data table
# ## computing expected frequencies
# f.ex<-c(20,71,61,31,17) ## expected frequencies vector
# f.os<-vector()
# for(i in 1:5) f.os[i] <- table(x.gam.cut)[[i]] ## empirical frequencies
# X2 <- sum(((f.os-f.ex)^2)/f.ex) ## chi-square statistic
# gdl <- 5-2-1 ## degrees of freedom
# #1-pchisq(X2,gdl) ## p-value
# 
# ## computing relative expected frequencies
# p<-c((pgamma(3,shape=3.5,rate=0.5)-pgamma(0,shape=3.5,rate=0.5)),
#      (pgamma(6,shape=3.5,rate=0.5)-pgamma(3,shape=3.5,rate=0.5)),
#      (pgamma(9,shape=3.5,rate=0.5)-pgamma(6,shape=3.5,rate=0.5)),
#      (pgamma(12,shape=3.5,rate=0.5)-pgamma(9,shape=3.5,rate=0.5)),
#      (pgamma(18,shape=3.5,rate=0.5)-pgamma(12,shape=3.5,rate=0.5)))
# chisq.test(x=f.os,p=p) ## chi-square test
# 
# # Kolmogorov-Smirnov goodness test
# ks.test(x.wei,"pweibull", shape=2,scale=1)
# 
# # Show true and approximated Weibull
# x <- seq(0,2,0.1)
# plot(x, pweibull(x,scale=1,shape=2),type="l",col="red", main="ECDF and Weibull CDF")
# plot(ecdf(x.wei),add=TRUE)
# 
# # Shapiro-Wilk test
# shapiro.test(x.norm)


